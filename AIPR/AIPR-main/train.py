"""
End-to-end training with optional Optuna HPO and automatic threshold
selection (Youden-J) when the --th flag is omitted.
"""
from __future__ import annotations

import argparse
import json
import logging
import random
from functools import lru_cache
from pathlib import Path

import numpy as np
import pandas as pd
import tensorflow as tf
from sklearn.metrics import (
    RocCurveDisplay,
    classification_report,
    confusion_matrix,
    roc_auc_score,
    roc_curve,
)
from sklearn.model_selection import train_test_split
from sklearn.utils.class_weight import compute_class_weight

# ─── Bayesian HPO ─────────────────────────────────────────────
try:
    import optuna

    _HAS_OPTUNA = True
except ImportError:
    _HAS_OPTUNA = False

from model import AI_PROB_THRESHOLD, IMG_SIZE, build_model

# ─── Paths & logging ────────────────────────────────────────────────────
ROOT = Path(__file__).resolve().parent
DATA_DIR = ROOT / "dataset"
MODEL_DIR = ROOT / "model"
MODEL_DIR.mkdir(exist_ok=True)

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s — %(levelname)s — %(message)s",
    handlers=[
        logging.FileHandler(MODEL_DIR / "train_results.txt", "w", "utf-8"),
        logging.StreamHandler(),
    ],
)
log = logging.getLogger(__name__)

# ─── Data augmentation  ────────────────
AUG_PIPE = tf.keras.Sequential(
    [
        tf.keras.layers.RandomFlip("horizontal"),
        tf.keras.layers.RandomRotation(0.17),  # ±10°
        tf.keras.layers.RandomZoom(0.10),
        tf.keras.layers.RandomContrast(0.10),
    ],
    name="augmentation",
)

_IMG_EXTS = (".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tif", ".tiff")

# ─── Helper: best ROC threshold (Youden-J) ──────────────────────────────
def _best_threshold(y_true: np.ndarray, y_prob: np.ndarray) -> float:
    """Return threshold that maximises TPR − FPR."""
    fpr, tpr, thr = roc_curve(y_true, y_prob)
    return float(thr[np.argmax(tpr - fpr)])

# ─── Helper: disk search (id → path) ────────────────────────────────────
@lru_cache(maxsize=100_000)
def _rglob_anywhere(name: str) -> Path | None:
    for p in DATA_DIR.rglob(name):
        if p.is_file():
            return p.resolve()
    return None

def _id_to_path(text: str) -> Path | None:
    """
    Map an *id* or file-name string from the CSV to a real file on disk.
    """
    raw = str(text).strip().lstrip("./")
    p_raw = Path(raw)

    if p_raw.is_absolute():
        return p_raw if p_raw.exists() else None

    if "/" in raw or p_raw.suffix.lower() in _IMG_EXTS:
        direct = (DATA_DIR / raw).resolve()
        return direct if direct.exists() else _rglob_anywhere(p_raw.name)

    for ext in _IMG_EXTS:
        hit = _rglob_anywhere(f"{raw}{ext}")
        if hit:
            return hit
    return None


def _load_df(csv: Path, *, expect_labels: bool) -> pd.DataFrame:
    """
    Read *csv* and resolve a valid ``filepath`` column (absolute Path objects).
    """
    df = pd.read_csv(csv)

    # unify the path column name
    for col in ("filepath", "file_name", "filename", "path"):
        if col in df.columns:
            df.rename(columns={col: "filepath"}, inplace=True)
            df["filepath"] = df["filepath"].apply(lambda p: Path(p).expanduser())
            break
    else:
        if "id" not in df.columns:
            raise KeyError(f"{csv.name}: missing both 'id' and a path column.")
        df["filepath"] = df["id"].apply(_id_to_path)

    # resolve and verify files
    df["filepath"] = df["filepath"].apply(
        lambda p: (DATA_DIR / p).resolve()
        if isinstance(p, Path) and not p.is_absolute()
        else p
    )
    df = df[df["filepath"].apply(lambda p: isinstance(p, Path) and p.exists())]
    if df.empty:
        raise RuntimeError(f"{csv.name}: no valid images after filtering.")

    # labels (if present)
    if "label" in df.columns:
        df["label"] = (
            df["label"].astype(str).str.lower().map({"human": 0, "0": 0, "ai": 1, "1": 1})
        )
        df = df.dropna(subset=["label"])
    elif expect_labels:
        raise KeyError(f"{csv.name}: missing 'label' column.")
    else:
        df["label"] = -1  # sentinel for unlabeled images

    log.info(
        "%s → %d images (Human=%d | AI=%d)",
        csv.name,
        len(df),
        (df.label == 0).sum(),
        (df.label == 1).sum(),
    )
    return df.reset_index(drop=True)


def _make_ds(df: pd.DataFrame, *, batch: int, augment: bool, shuffle: bool):
    """
    Build a tf.data pipeline from a dataframe.
    """
    paths = df["filepath"].astype(str).to_numpy()
    labels = df["label"].astype("float32").to_numpy()

    ds = tf.data.Dataset.from_tensor_slices((paths, labels))

    def _loader(path, lab):
        img = tf.io.read_file(path)
        img = tf.io.decode_image(img, channels=3, expand_animations=False)
        img = tf.image.resize_with_pad(img, *IMG_SIZE) / 255.0
        if augment:
            img = AUG_PIPE(img, training=True)
        return img, lab

    if shuffle:
        ds = ds.shuffle(len(df), seed=42, reshuffle_each_iteration=True)
    return ds.map(_loader, tf.data.AUTOTUNE).batch(batch).prefetch(tf.data.AUTOTUNE)


def _linear_lr(base_lr: float, base_batch: int, batch: int) -> float:
    """
    Simple linear LR scaling rule: lr ∝ batch_size.
    """
    return base_lr * batch / base_batch


def _build_callbacks(
    ckpt_path: Path,
    patience: int,
    verbose: int,
    tb: tf.keras.callbacks.TensorBoard | None = None,
):
    """
    Return common callbacks (EarlyStopping, ModelCheckpoint, CSVLogger, TB).
    """
    cbs: list[tf.keras.callbacks.Callback] = [
        tf.keras.callbacks.EarlyStopping(
            monitor="val_loss",
            patience=patience,
            restore_best_weights=True,
            verbose=verbose,
        ),
        tf.keras.callbacks.ModelCheckpoint(
            filepath=str(ckpt_path),
            monitor="val_loss",
            save_best_only=True,
        ),
        tf.keras.callbacks.CSVLogger(str(MODEL_DIR / "history.csv"), append=True),
    ]
    if tb:
        cbs.append(tb)
    return cbs


# ─── Optuna objective (quick 3-epoch proxy) ─────────────────────────────
def _objective(tr_df: pd.DataFrame, val_df: pd.DataFrame, trial: "optuna.Trial"):
    params = {
        "dropout": trial.suggest_float("dropout", 0.0, 0.5, step=0.05),
        "lr": trial.suggest_float("lr", 1e-4, 1e-2, log=True),
        "gamma": trial.suggest_float("gamma", 1.0, 3.0),
    }

    batch = 32
    tr_ds = _make_ds(tr_df, batch=batch, augment=True, shuffle=True)
    val_ds = _make_ds(val_df, batch=batch, augment=False, shuffle=False)

    model = build_model(
        train_backbone=False,
        learning_rate=_linear_lr(params["lr"], 32, batch),
    )
    model.get_layer("dropout").rate = params["dropout"]
    model.compile(
        optimizer=tf.keras.optimizers.Adam(_linear_lr(params["lr"], 32, batch)),
        loss=tf.keras.losses.BinaryFocalCrossentropy(gamma=params["gamma"]),
        metrics=[tf.keras.metrics.AUC(name="auc")],
    )
    model.fit(tr_ds, epochs=3, validation_data=val_ds, verbose=0)
    val_auc = model.evaluate(val_ds, verbose=0)[1]
    return 1.0 - val_auc  # minimise


# ─── Training pipeline ─────────────────────────────────────────────────
def train(cfg: argparse.Namespace):
    # Reproducibility
    random.seed(42)
    np.random.seed(42)
    tf.keras.utils.set_random_seed(42)
    for g in tf.config.list_physical_devices("GPU"):
        try:
            tf.config.experimental.set_memory_growth(g, True)
        except Exception:  # noqa: BLE001
            pass

    full_df = _load_df(DATA_DIR / "train.csv", expect_labels=True)
    tr_df, val_df = train_test_split(
        full_df, test_size=cfg.val_split, stratify=full_df["label"], random_state=42
    )

    # ── Hyper-parameter search ──────────────────────────────
    if cfg.tune:
        if not _HAS_OPTUNA:
            raise RuntimeError("Optuna not installed – run `pip install optuna`.")
        log.info("🔎  Optuna search (%d trials)…", cfg.tune_trials)
        study = optuna.create_study(direction="minimize")
        study.optimize(
            lambda t: _objective(tr_df, val_df, t),
            n_trials=cfg.tune_trials,
            show_progress_bar=True,
        )
        best = study.best_trial.params
        log.info("Best params: %s", best)
        cfg.lr = best["lr"]
        cfg.dropout = best["dropout"]
        cfg.focal_gamma = best["gamma"]

    # Data pipelines
    train_ds = _make_ds(tr_df, batch=cfg.batch, augment=True, shuffle=True)
    val_ds = _make_ds(val_df, batch=cfg.batch, augment=False, shuffle=False)

    # Class weights
    cw = compute_class_weight(
        "balanced", classes=np.array([0, 1]), y=tr_df["label"].values
    )
    cw_dict = {0: cw[0] * cfg.cw_human, 1: cw[1]}
    log.info("Stage-1 class-weight: %s", cw_dict)

    # Build or resume model
    ckpt = MODEL_DIR / "final_model.keras"
    model = (
        tf.keras.models.load_model(ckpt, compile=False)
        if cfg.resume and ckpt.exists()
        else build_model(
            train_backbone=False,
            learning_rate=_linear_lr(cfg.lr, 32, cfg.batch),
        )
    )
    if hasattr(cfg, "dropout"):  # set by Optuna
        model.get_layer("dropout").rate = cfg.dropout

    # TensorBoard callback
    tb_cb = (
        tf.keras.callbacks.TensorBoard(
            log_dir=str(MODEL_DIR / "tb"), write_graph=False, update_freq="epoch"
        )
        if cfg.tb
        else None
    )

    # ── Stage-1 (frozen backbone) ─────────────────────────────────────
    if not cfg.resume:
        log.info("🚀 Stage-1 (%d epochs)", cfg.epochs_stage1)
        model.fit(
            train_ds,
            epochs=cfg.epochs_stage1,
            validation_data=val_ds,
            class_weight=cw_dict,
            verbose=cfg.verbose,
            callbacks=_build_callbacks(
                ckpt, patience=3, verbose=cfg.verbose, tb=tb_cb
            ),
        )

    # ── Stage-2 (fine-tune) ───────────────────────────────────────────────
    log.info("🚀 Stage-2 (%d epochs, unfreeze last 10 layers)", cfg.epochs_stage2)
    for layer in model.layers[-10:]:
        layer.trainable = True

    ft_lr = _linear_lr(cfg.lr_finetune, 32, cfg.batch) * 0.2

    # --- give BinaryAccuracy a safe placeholder if cfg.th is None -----------
    metric_threshold = cfg.th if cfg.th is not None else AI_PROB_THRESHOLD
    # ------------------------------------------------------------------------

    model.compile(
        optimizer=tf.keras.optimizers.Adam(ft_lr),
        loss=tf.keras.losses.BinaryFocalCrossentropy(gamma=getattr(cfg, "focal_gamma", 2.0)),
        metrics=[
            tf.keras.metrics.BinaryAccuracy(threshold=metric_threshold),
            tf.keras.metrics.AUC(name="auroc"),
        ],
    )

    log.info("Stage-2 class-weight: %s", cw_dict)
    model.fit(
        train_ds,
        epochs=cfg.epochs_stage2,
        validation_data=val_ds,
        class_weight=cw_dict,
        verbose=cfg.verbose,
        callbacks=_build_callbacks(
            ckpt, patience=3, verbose=cfg.verbose, tb=tb_cb
        )
                  + [
                      tf.keras.callbacks.ReduceLROnPlateau(
                          monitor="val_loss", factor=0.5, patience=1, min_lr=1e-6, verbose=cfg.verbose
                      )
                  ],
    )

    # ── Validation metrics & ROC ────────────────────────────────────────────
    y_true, y_prob = [], []
    for xb, yb in val_ds:
        prob = model.predict(xb, verbose=0).flatten()
        y_true.extend(yb.numpy().astype(int))
        y_prob.extend(prob)

    # — choose threshold if not given —
    if cfg.th is None:
        cfg.th = _best_threshold(np.array(y_true), np.array(y_prob))
        log.info("Auto-selected threshold (Youden-J) = %.3f", cfg.th)

    y_pred = (np.array(y_prob) >= cfg.th).astype(int)

    log.info("\nConfusion matrix\n%s", confusion_matrix(y_true, y_pred))
    log.info(
        "\nClassification report\n%s",
        classification_report(y_true, y_pred, target_names=["Human", "AI"], digits=3),
    )
    val_auc = roc_auc_score(y_true, y_prob)
    log.info("Validation AUC: %.4f", val_auc)

    RocCurveDisplay.from_predictions(
        y_true,
        y_prob,
        name=f"Val ROC (AUC = {val_auc:.2f})",
        color="darkorange",
    ).figure_.savefig(MODEL_DIR / "roc_curve.png", dpi=120)

    # ─── Persist Optuna params *plus* the chosen threshold ───────────
    best_dict = dict(best) if cfg.tune else {}
    best_dict["threshold"] = float(cfg.th)
    (MODEL_DIR / "best_params.json").write_text(json.dumps(best_dict, indent=2))


# ─── CLI ────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--epochs_stage1", type=int, default=5)
    parser.add_argument("--epochs_stage2", type=int, default=10)
    parser.add_argument("--batch", type=int, default=32)
    parser.add_argument("--lr", type=float, default=1e-3)
    parser.add_argument("--lr_finetune", type=float, default=1e-5)
    parser.add_argument("--val_split", type=float, default=0.2)
    parser.add_argument("--resume", action="store_true")
    parser.add_argument("--cw_human", type=float, default=1.0)
    parser.add_argument(
        "--th",
        type=float,
        default=None,
        help="Decision threshold; omit to pick best ROC point automatically.",
    )
    parser.add_argument("--tb", action="store_true")
    parser.add_argument("--verbose", type=int, choices=[0, 1, 2], default=1)
    # Optuna flags
    parser.add_argument("--tune", action="store_true")
    parser.add_argument("--tune_trials", type=int, default=30)

    train(parser.parse_args())
