"""
Model utilities and global constants.

* Loads the **dynamic decision threshold** from ``model/best_params.json``.
* Provides ``build_model()`` for training / inference.
"""
from __future__ import annotations

import json
from pathlib import Path
from typing import Tuple

import tensorflow as tf

# ────────────────────────────────────────────────
# 1.  Constants
# ────────────────────────────────────────────────
IMG_SIZE: Tuple[int, int] = (224, 224)  # Height, Width

_cfg_path = Path(__file__).resolve().parent.parent / "model" / "best_params.json"
if _cfg_path.exists():
    AI_PROB_THRESHOLD: float = float(json.loads(_cfg_path.read_text()).get("threshold", 0.5))
else:
    AI_PROB_THRESHOLD: float = 0.5  # first run / no tuning yet


# ────────────────────────────────────────────────
# 2.  Model factory
# ────────────────────────────────────────────────
def build_model(
    *,
    train_backbone: bool = False,
    learning_rate: float = 1e-3,
    threshold: float | None = None,
) -> tf.keras.Model:
    """
    EfficientNet-B0 → global-avg-pool → dropout → 1-unit sigmoid.
    """
    th = AI_PROB_THRESHOLD if threshold is None else threshold

    base = tf.keras.applications.EfficientNetB0(
        include_top=False,
        weights="imagenet",
        input_shape=(*IMG_SIZE, 3),
        pooling="avg",
    )
    base.trainable = train_backbone

    x = tf.keras.layers.Dropout(0.3, name="dropout")(base.output)
    out = tf.keras.layers.Dense(1, activation="sigmoid", name="head")(x)

    model = tf.keras.Model(base.input, out)
    model.compile(
        optimizer=tf.keras.optimizers.Adam(learning_rate),
        loss="binary_crossentropy",
        metrics=[
            tf.keras.metrics.BinaryAccuracy(threshold=th),
            tf.keras.metrics.AUC(name="AUC"),
        ],
    )
    return model
