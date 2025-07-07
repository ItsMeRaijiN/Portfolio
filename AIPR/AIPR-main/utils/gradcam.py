"""
Grad-CAM helper:

* Uses **tf-explain** if it is installed (preferred and faster).
* Falls back to a pure-TensorFlow implementation otherwise.
* Returns an RGB uint8 image with the heat-map blended onto the input.
"""
from __future__ import annotations

from typing import Callable, Optional

import matplotlib.cm as mpl_cm
import numpy as np
import tensorflow as tf

# ────────────────────────────────────────────────
# 1.  Optional tf-explain backend
# ────────────────────────────────────────────────
def _build_tf_explain() -> Optional[Callable]:
    """
    If tf-explain is available, return a heat-map function that wraps it.
    Otherwise return ``None`` so we can fall back.
    """
    try:
        from tf_explain.core.grad_cam import GradCAM as _TfExplain  # type: ignore
    except Exception:  # tf-explain not installed or import error
        return None

    def _heat_tf_explain(
        model: tf.keras.Model,
        img: tf.Tensor,
        layer: str,
    ) -> tf.Tensor:
        """
        Run tf-explain and return a (H, W) float32 heat-map in [0, 1].
        """
        explainer = _TfExplain()

        # Signature changed between tf-explain versions
        try:
            heat_rgb = explainer.explain(
                validation_data=(img.numpy(), None),
                model=model,
                class_index=0,
                layer_name=layer,
            )
        except TypeError:
            # Old signature: (val_data, model, layer_name, class_index)
            heat_rgb = explainer.explain((img.numpy(), None), model, layer, 0)

        heat = tf.image.convert_image_dtype(heat_rgb, tf.float32)    # 0-1
        heat = tf.image.rgb_to_grayscale(heat)[..., 0]               # (H, W)
        return heat

    return _heat_tf_explain


_HEAT_FN: Optional[Callable] = _build_tf_explain()

# ────────────────────────────────────────────────
# 2.  Pure-TensorFlow fallback
# ────────────────────────────────────────────────
def _heat_gradcam_raw(
    model: tf.keras.Model,
    img: tf.Tensor,
    layer_name: str,
) -> tf.Tensor:
    """
    Classic Grad-CAM implementation.
    Returns a (H, W) float32 heat-map in [0, 1].
    ``img`` must be a (1, H, W, C) tensor in [0, 1].
    """
    conv_layer = model.get_layer(layer_name)

    grad_model = tf.keras.Model(
        inputs=model.inputs,
        outputs=(conv_layer.output, model.output),
    )

    with tf.GradientTape() as tape:
        activations, preds = grad_model(img)
        class_channel = preds[:, 0]  # binary problem → class index 0

    grads = tape.gradient(class_channel, activations)
    pooled = tf.reduce_mean(grads, axis=(0, 1, 2))

    activations = activations[0]                         # (h′, w′, f)
    heat = tf.reduce_sum(tf.multiply(pooled, activations), axis=-1)

    heat = tf.maximum(heat, 0)
    heat = heat / (tf.reduce_max(heat) + 1e-8)
    heat = tf.image.resize(heat[..., tf.newaxis], img.shape[1:3])
    return tf.squeeze(heat, axis=-1)                     # (H, W)

# ────────────────────────────────────────────────
# 3.  Public API
# ────────────────────────────────────────────────
def generate_gradcam_overlay(
    model: tf.keras.Model,
    img_tensor: tf.Tensor,              # (1, H, W, C) – values in [0, 1]
    target_layer: str | None = None,
    alpha: float = 0.35,
    colormap: str = "inferno",
) -> np.ndarray:
    """
    Blend a Grad-CAM heat-map onto the input image and return an RGB ``uint8``
    array ready for display.
    """
    # Pick the last Conv2D layer if none specified.
    if target_layer is None:
        for lyr in reversed(model.layers):
            if isinstance(lyr, tf.keras.layers.Conv2D):
                target_layer = lyr.name
                break
        if target_layer is None:
            raise ValueError("No Conv2D layer found in the supplied model.")

    # Compute the heat-map.
    if _HEAT_FN is not None:
        heat = _HEAT_FN(model, img_tensor, target_layer)
    else:
        heat = _heat_gradcam_raw(model, img_tensor, target_layer)

    heat = heat.numpy()                                    # (H, W)

    # Map to RGB and blend.
    cmap = mpl_cm.get_cmap(colormap)
    heat_rgb = (cmap(heat)[..., :3] * 255).astype(np.uint8)
    img_rgb = (img_tensor[0].numpy() * 255).astype(np.uint8)

    overlay = (alpha * heat_rgb + (1.0 - alpha) * img_rgb).astype(np.uint8)
    return overlay
