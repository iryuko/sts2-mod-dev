"""Decode uploaded images without changing the original image bytes."""
import hashlib
from io import BytesIO
import warnings
from PIL import Image, UnidentifiedImageError
from workbench_model import ValidationError

MAX_BYTES=10*1024*1024
MAX_PIXELS=32_000_000
FORMATS={'PNG':('image/png','png'),'JPEG':('image/jpeg','jpg'),'WEBP':('image/webp','webp')}


def validate_image(data):
    if not data or len(data)>MAX_BYTES: raise ValidationError('图片大小需在10MiB以内','image_size')
    try:
        with warnings.catch_warnings():
            warnings.simplefilter('error',Image.DecompressionBombWarning)
            with Image.open(BytesIO(data)) as image:
                fmt=image.format;width,height=image.size
                if fmt not in FORMATS: raise ValidationError('仅支持PNG、JPEG和WebP','image_type')
                if width*height>MAX_PIXELS: raise ValidationError('图片不能超过3200万像素','image_pixels')
                image.verify()
            with Image.open(BytesIO(data)) as image: image.load()
    except (UnidentifiedImageError,OSError,ValueError,Image.DecompressionBombError,Image.DecompressionBombWarning) as error:
        if isinstance(error,ValidationError): raise
        raise ValidationError('无法解码图片','image_type') from error
    mime,extension=FORMATS[fmt]
    return {'format':fmt,'mime':mime,'extension':extension,'width':width,'height':height,
            'sha256':hashlib.sha256(data).hexdigest()}
