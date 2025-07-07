#!/usr/bin/env python3
"""
Gradio front-end: upload an image, the model decides *AI vs Human*,
shows a Grad-CAM overlay and displays the current decision threshold.
"""
from __future__ import annotations

from pathlib import Path

import gradio as gr
import numpy as np
import tensorflow as tf
from PIL import Image

from model import AI_PROB_THRESHOLD, IMG_SIZE
from utils.gradcam import generate_gradcam_overlay

MODEL_PATH = Path("model/final_model.keras")
MODEL = tf.keras.models.load_model(MODEL_PATH, compile=False)

ALPHA = 0.35


# ────────────────────────────────────────────────
# 1.  Pre-processing
# ────────────────────────────────────────────────
def _preprocess(pil_img: Image.Image) -> np.ndarray:
    """
    Convert a PIL image into a NumPy batch (1, H, W, C) in [0, 1].
    Returning a plain array avoids Keras' nested-structure warning.
    """
    pil_img = pil_img.convert("RGB").resize(IMG_SIZE)
    arr = np.asarray(pil_img, dtype=np.float32) / 255.0
    return np.expand_dims(arr, 0)   # (1, H, W, C)


# ────────────────────────────────────────────────
# 2.  Inference + Grad-CAM
# ────────────────────────────────────────────────
def infer(pil_img: Image.Image) -> tuple[str, np.ndarray | None]:
    try:
        x_np = _preprocess(pil_img)
        # Feed as a list – matches the model's expected input structure.
        prob: float = float(MODEL.predict([x_np], verbose=0)[0][0])
        decision = "AI" if prob >= AI_PROB_THRESHOLD else "Human"

        x_tf = tf.convert_to_tensor(x_np, dtype=tf.float32)
        cam = generate_gradcam_overlay(MODEL, x_tf, alpha=ALPHA)

        return (
            f"{decision} (p = {prob:.3f})",
            cam,
        )
    except Exception as exc:
        return f"Error: {exc}", None


# ────────────────────────────────────────────────
# 3.  Gradio UI
# ────────────────────────────────────────────────
demo = gr.Interface(
    fn=infer,
    inputs=gr.Image(type="pil", label="Upload image"),
    outputs=[
        gr.Textbox(label="Decision"),
        gr.Image(label="Grad-CAM"),
    ],
    title="AI vs Human Image Detector",
    flagging_mode="never",
    examples=None,
    cache_examples=False,
    article=f"**Current decision threshold:** {AI_PROB_THRESHOLD:.3f}",
)

if __name__ == "__main__":
    demo.launch()
