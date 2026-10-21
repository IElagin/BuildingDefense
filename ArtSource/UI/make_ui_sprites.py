"""BuildingDefense L8 UI sprites (sci-fi outpost style), drawn with Pillow at 4x and downsampled.

Usage: python3 ArtSource/UI/make_ui_sprites.py  ->  Assets/_Project/Art/Sprites/UI/*.png
Panels, buttons, bars and the banner are 9-slice sources (borders listed in BORDERS for the Unity importer).
Banner, Glow and Vignette are light so Unity tints them per screen. The menu background is the accepted
mood frame ArtSource/Reference/swarm_mood_v1.jpg scaled to 2048 px -> Art/Sprites/Backgrounds.
Colours follow the game palette (ArtSource/Palette/palette.png): teal 2FD3C8, deep teal 127A86,
cyan glow 8AF6FF, orange FF8A1F / FF7A1A, gold FFD34D, chitin 1E1626, toxic green 7CFF3A.
"""
from pathlib import Path

import math

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/_Project/Art/Sprites/UI"
BACKGROUNDS = ROOT / "Assets/_Project/Art/Sprites/Backgrounds"
SS = 4  # supersampling factor

# 9-slice borders in final pixels (left, bottom, right, top) for the importer
BORDERS = {
    "Panel": (22, 22, 22, 22),
    "PanelHeader": (22, 22, 22, 22),
    "ButtonTeal": (20, 20, 20, 20),
    "ButtonOrange": (20, 20, 20, 20),
    "ButtonDark": (20, 20, 20, 20),
    "BarBack": (10, 10, 10, 10),
    "BarFill": (8, 8, 8, 8),
    "Banner": (44, 0, 44, 0),
}


def hexc(h, a=255):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def chamfer_poly(w, h, c, inset=0.0):
    """Rectangle with 45° cut corners (top-left and bottom-right cut larger, sci-fi look)."""
    i = inset
    big, small = c, c * 0.45
    return [
        (i + big, i), (w - i - small, i), (w - i, i + small),
        (w - i, h - i - big), (w - i - big, h - i), (i + small, h - i),
        (i, h - i - small), (i, i + big),
    ]


def vertical_gradient(size, top, bottom):
    w, h = size
    grad = Image.new("RGBA", size)
    px = grad.load()
    for y in range(h):
        t = y / max(1, h - 1)
        col = tuple(int(top[k] + (bottom[k] - top[k]) * t) for k in range(4))
        for x in range(w):
            px[x, y] = col
    return grad


def save(img, name):
    final = img.resize((img.width // SS, img.height // SS), Image.LANCZOS)
    OUT.mkdir(parents=True, exist_ok=True)
    final.save(OUT / f"{name}.png")
    print(name, final.size)


def panel(name, fill_top, fill_bottom, border, header=None, size=(96, 96), chamfer=18):
    w, h = size[0] * SS, size[1] * SS
    c = chamfer * SS
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).polygon(chamfer_poly(w, h, c), fill=255)
    body = vertical_gradient((w, h), fill_top, fill_bottom)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    img.paste(body, (0, 0), mask)
    d = ImageDraw.Draw(img)
    if header:
        hm = Image.new("L", (w, h), 0)
        ImageDraw.Draw(hm).polygon(chamfer_poly(w, h, c), fill=255)
        ImageDraw.Draw(hm).rectangle((0, 30 * SS, w, h), fill=0)
        img.paste(Image.new("RGBA", (w, h), header), (0, 0), hm)
    d.line(chamfer_poly(w, h, c, inset=1.5 * SS) + [chamfer_poly(w, h, c, inset=1.5 * SS)[0]], fill=border, width=int(2.2 * SS))
    # small corner ticks for a technical look
    d.line([(c + 6 * SS, 5 * SS), (c + 22 * SS, 5 * SS)], fill=border, width=int(1.6 * SS))
    d.line([(w - c - 22 * SS, h - 5 * SS), (w - c - 6 * SS, h - 5 * SS)], fill=border, width=int(1.6 * SS))
    save(img, name)


def button(name, top, bottom, outline, highlight, size=(96, 72), chamfer=16):
    w, h = size[0] * SS, size[1] * SS
    c = chamfer * SS
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).polygon(chamfer_poly(w, h, c), fill=255)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    img.paste(vertical_gradient((w, h), top, bottom), (0, 0), mask)
    d = ImageDraw.Draw(img)
    # top highlight band
    hl = Image.new("L", (w, h), 0)
    ImageDraw.Draw(hl).polygon(chamfer_poly(w, h, c, inset=4 * SS), fill=255)
    ImageDraw.Draw(hl).rectangle((0, h * 0.42, w, h), fill=0)
    band = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    band.paste(Image.new("RGBA", (w, h), highlight), (0, 0), hl)
    img = Image.alpha_composite(img, band)
    d = ImageDraw.Draw(img)
    poly = chamfer_poly(w, h, c, inset=1.5 * SS)
    d.line(poly + [poly[0]], fill=outline, width=int(3 * SS))
    save(img, name)


def bar(name, fill_top, fill_bottom, border=None, size=(48, 32), radius=7):
    w, h = size[0] * SS, size[1] * SS
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, w - 1, h - 1), radius=radius * SS, fill=255)
    img.paste(vertical_gradient((w, h), fill_top, fill_bottom), (0, 0), mask)
    if border:
        ImageDraw.Draw(img).rounded_rectangle((SS, SS, w - SS - 1, h - SS - 1), radius=radius * SS, outline=border, width=int(2 * SS))
    save(img, name)


def hexagon(cx, cy, r, flat=True):
    start = 0 if flat else 30
    return [(cx + r * math.cos(math.radians(start + 60 * k)), cy + r * math.sin(math.radians(start + 60 * k))) for k in range(6)]


def icon_gold(size=128):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = s / 2
    d.polygon(hexagon(c, c + 2 * SS, s * 0.46), fill=hexc("8C5A14"))
    d.polygon(hexagon(c, c, s * 0.44), fill=hexc("E0A82A"))
    d.polygon(hexagon(c, c - 2 * SS, s * 0.36), fill=hexc("FFD34D"))
    d.polygon(hexagon(c, c - 2 * SS, s * 0.2), fill=hexc("FFE89A"))
    d.polygon(hexagon(c, c - 2 * SS, s * 0.12), fill=hexc("E0A82A"))
    save(img, "IconGold")


def icon_shield(size=128):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    pts = [(s * 0.5, s * 0.06), (s * 0.88, s * 0.2), (s * 0.82, s * 0.6), (s * 0.5, s * 0.94), (s * 0.18, s * 0.6), (s * 0.12, s * 0.2)]
    d.polygon(pts, fill=hexc("0B3B40"))
    inner = [(x * 0.82 + s * 0.09, y * 0.82 + s * 0.08) for x, y in pts]
    d.polygon(inner, fill=hexc("2FD3C8"))
    half = [inner[0], inner[1], inner[2], inner[3]]
    d.polygon(half, fill=hexc("8AF6FF"))
    # dome glyph
    d.pieslice((s * 0.32, s * 0.36, s * 0.68, s * 0.72), 180, 360, fill=hexc("0B3B40"))
    d.rectangle((s * 0.3, s * 0.53, s * 0.7, s * 0.58), fill=hexc("0B3B40"))
    save(img, "IconShield")


def icon_bug(size=128):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    w = int(6 * SS)
    for side in (-1, 1):
        for k, (ky, fy) in enumerate(((0.38, 0.2), (0.5, 0.52), (0.6, 0.86))):
            x0, y0 = s / 2, s * (0.4 + 0.08 * k)
            kx, kyy = s / 2 + side * s * 0.28, s * ky
            fx, fyy = s / 2 + side * s * 0.44, s * fy
            d.line([(x0, y0), (kx, kyy), (fx, fyy)], fill=hexc("1E1626"), width=w)
    d.ellipse((s * 0.3, s * 0.42, s * 0.7, s * 0.9), fill=hexc("2E2140"))
    d.ellipse((s * 0.36, s * 0.5, s * 0.64, s * 0.84), fill=hexc("7CFF3A"))
    d.line([(s * 0.5, s * 0.5), (s * 0.5, s * 0.84)], fill=hexc("2E2140"), width=int(4 * SS))
    d.ellipse((s * 0.38, s * 0.22, s * 0.62, s * 0.46), fill=hexc("2E2140"))
    d.ellipse((s * 0.41, s * 0.29, s * 0.47, s * 0.35), fill=hexc("FF8A1F"))
    d.ellipse((s * 0.53, s * 0.29, s * 0.59, s * 0.35), fill=hexc("FF8A1F"))
    save(img, "IconBug")


def icon_timer(size=128):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse((s * 0.12, s * 0.16, s * 0.88, s * 0.92), fill=hexc("0B3B40"))
    d.ellipse((s * 0.18, s * 0.22, s * 0.82, s * 0.86), fill=hexc("2FD3C8"))
    d.rectangle((s * 0.42, s * 0.04, s * 0.58, s * 0.16), fill=hexc("0B3B40"))
    d.pieslice((s * 0.24, s * 0.28, s * 0.76, s * 0.8), -90, 30, fill=hexc("8AF6FF"))
    d.line([(s * 0.5, s * 0.54), (s * 0.5, s * 0.34)], fill=hexc("0B3B40"), width=int(6 * SS))
    d.line([(s * 0.5, s * 0.54), (s * 0.64, s * 0.62)], fill=hexc("0B3B40"), width=int(6 * SS))
    save(img, "IconTimer")


def glow_dot(size=64):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    ImageDraw.Draw(img).ellipse((s * 0.2, s * 0.2, s * 0.8, s * 0.8), fill=(255, 255, 255, 255))
    img = img.filter(ImageFilter.GaussianBlur(s * 0.08))
    save(img, "Glow")


def star_points(cx, cy, outer, inner, n=5):
    pts = []
    for k in range(n * 2):
        r = outer if k % 2 == 0 else inner
        a = math.radians(-90 + 180 / n * k)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def star(name, outline, fill, top=None, size=128):
    s = size * SS
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = s / 2
    d.polygon(star_points(c, c + 3 * SS, s * 0.48, s * 0.22), fill=outline)
    d.polygon(star_points(c, c + 3 * SS, s * 0.4, s * 0.18), fill=fill)
    if top:
        # upper facets lighter: the flat low-poly shading of the game
        pts = star_points(c, c + 3 * SS, s * 0.4, s * 0.18)
        for k in range(0, 10, 2):
            d.polygon([(c, c + 3 * SS), pts[k], pts[(k + 1) % 10]], fill=top)
    save(img, name)


def banner(size=(160, 80), cut=30):
    """Header plate sliced only horizontally: chevron ends, light so the popup tints it."""
    w, h = size[0] * SS, size[1] * SS
    c = cut * SS
    poly = [(c, 0), (w - c, 0), (w, h / 2), (w - c, h), (c, h), (0, h / 2)]
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).polygon(poly, fill=255)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    img.paste(vertical_gradient((w, h), hexc("FFFFFF"), hexc("B4B4B4")), (0, 0), mask)
    d = ImageDraw.Draw(img)
    inner = [(c + 3 * SS, 6 * SS), (w - c - 3 * SS, 6 * SS), (w - 9 * SS, h / 2), (w - c - 3 * SS, h - 6 * SS),
             (c + 3 * SS, h - 6 * SS), (9 * SS, h / 2)]
    d.line(inner + [inner[0]], fill=hexc("6E6E6E"), width=int(2 * SS))
    save(img, "Banner")


def vignette(size=256):
    """Transparent centre, opaque edges; tinted black over the menu background."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    for y in range(size):
        for x in range(size):
            dx, dy = (x + 0.5) / size * 2 - 1, (y + 0.5) / size * 2 - 1
            r = math.sqrt(dx * dx + dy * dy) / math.sqrt(2)
            a = max(0.0, min(1.0, (r - 0.35) / 0.65)) ** 1.6
            px[x, y] = (255, 255, 255, int(a * 255))
    OUT.mkdir(parents=True, exist_ok=True)
    img.save(OUT / "Vignette.png")
    print("Vignette", img.size)


def menu_background(width=2048):
    src = Image.open(ROOT / "ArtSource/Reference/swarm_mood_v1.jpg").convert("RGB")
    img = src.resize((width, round(src.height * width / src.width)), Image.LANCZOS)
    BACKGROUNDS.mkdir(parents=True, exist_ok=True)
    img.save(BACKGROUNDS / "MenuBackground.jpg", quality=90)
    print("MenuBackground", img.size)


if __name__ == "__main__":
    panel("Panel", hexc("1C2833", 235), hexc("111A22", 235), hexc("2FD3C8"))
    panel("PanelHeader", hexc("1C2833", 245), hexc("111A22", 245), hexc("2FD3C8"), header=hexc("127A86", 255))
    button("ButtonTeal", hexc("4BE6D9"), hexc("139AA3"), hexc("0B3B40"), hexc("FFFFFF", 46))
    button("ButtonOrange", hexc("FFB04A"), hexc("E0661A"), hexc("5A2414"), hexc("FFFFFF", 52))
    button("ButtonDark", hexc("2A3A46"), hexc("1A242C"), hexc("2FD3C8"), hexc("FFFFFF", 22))
    bar("BarBack", hexc("0D141B", 235), hexc("0D141B", 235), border=hexc("2A3A46"))
    bar("BarFill", hexc("FFFFFF"), hexc("C8C8C8"), size=(40, 24), radius=6)
    icon_gold()
    icon_shield()
    icon_bug()
    icon_timer()
    glow_dot()
    star("Star", hexc("8C5A14"), hexc("E0A82A"), top=hexc("FFD34D"))
    star("StarSlot", hexc("0B3B40"), hexc("1C2833"))
    banner()
    vignette()
    menu_background()
    print("borders", BORDERS)
