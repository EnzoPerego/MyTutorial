"""Gera os sprites próprios do Fuga na Baía no estilo flat do Kenney Pirate Pack.

Uso: python3 Tools/gen_art.py Assets/Art/Custom
Todos os sprites são desenhados a 4x e reduzidos (antialiasing). Escala final: 128 px por unidade.
"""
import math
import os
import sys

from PIL import Image, ImageDraw, ImageFilter

SS = 4  # supersampling


def canvas(w, h):
    img = Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def s(v):
    return v * SS


def box(x0, y0, x1, y1):
    return [s(x0), s(y0), s(x1), s(y1)]


def finish(img, w, h):
    return img.resize((w, h), Image.LANCZOS)


def save(img, out, name):
    img.save(os.path.join(out, name))


# ---------------------------------------------------------------- personagens

def surfer():
    w, h = 96, 168
    img, d = canvas(w, h)
    # prancha (nariz para cima)
    board = [(48, 4), (66, 22), (74, 60), (74, 120), (66, 152), (48, 164), (30, 152), (22, 120), (22, 60), (30, 22)]
    d.polygon([(s(x), s(y)) for x, y in board], fill="#2ec4b6", outline="#1b7f76", width=s(4))
    d.line([s(48), s(14), s(48), s(156)], fill="#ffffff", width=s(5))
    d.line([s(40), s(130), s(56), s(130)], fill="#ff9f1c", width=s(6))
    d.line([s(40), s(40), s(56), s(40)], fill="#ff9f1c", width=s(6))
    # braços abertos (de lado na prancha)
    d.rounded_rectangle(box(6, 76, 90, 90), radius=s(7), fill="#f1c27d", outline="#b07d48", width=s(3))
    # ombros / camiseta
    d.ellipse(box(26, 68, 70, 100), fill="#e63946", outline="#9d1f2b", width=s(3))
    # cabeça e cabelo
    d.ellipse(box(35, 70, 61, 96), fill="#f1c27d", outline="#b07d48", width=s(3))
    d.pieslice(box(35, 70, 61, 96), 180, 360, fill="#6b3e1e")
    d.ellipse(box(35, 70, 61, 96), outline="#4a2a12", width=s(3))
    return finish(img, w, h)


def shark():
    w, h = 104, 232
    body_layer, d = canvas(w, h)
    # corpo submerso (translúcido)
    body = [(52, 6), (70, 40), (76, 90), (72, 140), (60, 180), (52, 196), (44, 180), (32, 140), (28, 90), (34, 40)]
    d.polygon([(s(x), s(y)) for x, y in body], fill=(70, 105, 130, 150))
    # nadadeiras peitorais
    d.polygon([(s(x), s(y)) for x, y in [(30, 80), (4, 120), (34, 108)]], fill=(70, 105, 130, 150))
    d.polygon([(s(x), s(y)) for x, y in [(74, 80), (100, 120), (70, 108)]], fill=(70, 105, 130, 150))
    # cauda
    d.polygon([(s(x), s(y)) for x, y in [(52, 186), (24, 226), (52, 212), (80, 226)]], fill=(70, 105, 130, 150))
    # barbatana dorsal (fora d'água, sólida)
    fin = [(52, 70), (66, 120), (52, 128), (38, 120)]
    d.polygon([(s(x), s(y)) for x, y in fin], fill="#3d5a73", outline="#22364a", width=s(4))
    d.line([s(52), s(76), s(52), s(122)], fill="#7fa3bf", width=s(3))
    # espuma em volta da barbatana
    d.arc(box(30, 112, 74, 140), 20, 160, fill=(255, 255, 255, 220), width=s(4))
    return finish(body_layer, w, h)


# ---------------------------------------------------------------- itens

def chest():
    w, h = 112, 96
    img, d = canvas(w, h)
    d.rounded_rectangle(box(8, 30, 104, 90), radius=s(8), fill="#a0612b", outline="#5c3417", width=s(5))
    d.rounded_rectangle(box(8, 8, 104, 44), radius=s(16), fill="#c07a3a", outline="#5c3417", width=s(5))
    for x in (22, 82):
        d.rectangle(box(x, 10, x + 10, 88), fill="#f4c542", outline="#a77b12", width=s(2))
    d.line([s(10), s(44), s(102), s(44)], fill="#5c3417", width=s(5))
    d.rounded_rectangle(box(46, 36, 66, 62), radius=s(4), fill="#f4c542", outline="#a77b12", width=s(3))
    d.ellipse(box(53, 44, 59, 50), fill="#5c3417")
    d.line([s(20), s(18), s(40), s(14)], fill=(255, 255, 255, 120), width=s(4))
    return finish(img, w, h)


def coin():
    w = h = 64
    img, d = canvas(w, h)
    d.ellipse(box(3, 3, 61, 61), fill="#f7c948", outline="#b7841a", width=s(5))
    d.ellipse(box(14, 14, 50, 50), outline="#e0a92a", width=s(4))
    d.rectangle(box(29, 20, 35, 44), fill="#e0a92a")
    d.arc(box(10, 10, 54, 54), 200, 250, fill=(255, 255, 255, 230), width=s(5))
    return finish(img, w, h)


def hourglass():
    w, h = 72, 104
    img, d = canvas(w, h)
    glass = [(14, 14), (58, 14), (58, 22), (40, 52), (58, 82), (58, 90), (14, 90), (14, 82), (32, 52), (14, 22)]
    d.polygon([(s(x), s(y)) for x, y in glass], fill=(200, 240, 255, 200), outline="#5a8aa6", width=s(3))
    d.polygon([(s(x), s(y)) for x, y in [(20, 24), (52, 24), (36, 48)]], fill="#f2c14e")
    d.polygon([(s(x), s(y)) for x, y in [(18, 86), (54, 86), (36, 66)]], fill="#f2c14e")
    d.line([s(36), s(50), s(36), s(70)], fill="#f2c14e", width=s(2))
    for y0 in (4, 88):
        d.rounded_rectangle(box(6, y0, 66, y0 + 12), radius=s(4), fill="#a0612b", outline="#5c3417", width=s(3))
    d.line([s(20), s(26), s(24), s(40)], fill=(255, 255, 255, 200), width=s(3))
    return finish(img, w, h)


def buoy():
    w = h = 96
    img, d = canvas(w, h)
    c, r_out, r_in = 48, 44, 22
    for i in range(8):
        col = "#e63946" if i % 2 == 0 else "#ffffff"
        d.pieslice(box(c - r_out, c - r_out, c + r_out, c + r_out), i * 45 - 90, (i + 1) * 45 - 90, fill=col)
    d.ellipse(box(c - r_out, c - r_out, c + r_out, c + r_out), outline="#8f1d27", width=s(4))
    d.ellipse(box(c - r_in, c - r_in, c + r_in, c + r_in), fill=(0, 0, 0, 0), outline="#8f1d27", width=s(4))
    # recorta o furo
    hole = Image.new("L", img.size, 255)
    ImageDraw.Draw(hole).ellipse(box(c - r_in + 2, c - r_in + 2, c + r_in - 2, c + r_in - 2), fill=0)
    img.putalpha(Image.composite(img.getchannel("A"), hole, hole))
    d.arc(box(c - 36, c - 36, c + 36, c + 36), 200, 250, fill=(255, 255, 255, 200), width=s(4))
    return finish(img, w, h)


def shield():
    w = h = 208
    img, d = canvas(w, h)
    d.ellipse(box(6, 6, 202, 202), fill=(120, 220, 255, 50), outline=(120, 220, 255, 220), width=s(7))
    d.arc(box(22, 22, 186, 186), 200, 260, fill=(255, 255, 255, 230), width=s(7))
    return finish(img, w, h)


def arrow():
    w, h = 64, 60
    img, d = canvas(w, h)
    pts = [(32, 3), (60, 54), (32, 42), (4, 54)]
    d.polygon([(s(x), s(y)) for x, y in pts], fill="#ffd23f", outline="#7a5200", width=s(5))
    return finish(img, w, h)


# ---------------------------------------------------------------- efeitos

def soft_circle(size, color):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    c = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            dist = math.hypot(x - c, y - c) / c
            a = max(0.0, 1.0 - dist) ** 1.5
            px[x, y] = color + (int(255 * a),)
    return img


def wave_line():
    w, h = 160, 40
    img, d = canvas(w, h)
    d.arc(box(10, 8, 80, 60), 200, 340, fill=(255, 255, 255, 255), width=s(6))
    d.arc(box(80, 8, 150, 60), 200, 340, fill=(255, 255, 255, 255), width=s(6))
    return finish(img, w, h)


def ring():
    w = h = 128
    img, d = canvas(w, h)
    d.ellipse(box(6, 6, 122, 122), outline=(255, 255, 255, 255), width=s(8))
    return finish(img, w, h)


# ---------------------------------------------------------------- interface (9-slice)

def panel():
    w = h = 128
    img, d = canvas(w, h)
    d.rounded_rectangle(box(2, 2, 126, 126), radius=s(28), fill="#f6e7c1", outline="#7a4a22", width=s(7))
    d.rounded_rectangle(box(14, 14, 114, 114), radius=s(18), outline="#d9c08f", width=s(3))
    return finish(img, w, h)


def button():
    w = h = 96
    img, d = canvas(w, h)
    d.rounded_rectangle(box(2, 6, 94, 94), radius=s(22), fill="#7a4520")
    d.rounded_rectangle(box(2, 2, 94, 88), radius=s(22), fill="#c07a3a", outline="#5c3417", width=s(5))
    d.rounded_rectangle(box(10, 10, 86, 40), radius=s(14), fill="#d6955a")
    return finish(img, w, h)


def plaque():
    w = h = 96
    img, d = canvas(w, h)
    d.rounded_rectangle(box(2, 2, 94, 94), radius=s(24), fill=(40, 30, 20, 150), outline=(255, 255, 255, 90), width=s(3))
    return finish(img, w, h)


def bar_bg():
    w, h = 96, 40
    img, d = canvas(w, h)
    d.rounded_rectangle(box(2, 2, 94, 38), radius=s(18), fill=(30, 20, 10, 170), outline="#5c3417", width=s(4))
    return finish(img, w, h)


def bar_fill():
    w, h = 96, 40
    img, d = canvas(w, h)
    d.rounded_rectangle(box(2, 2, 94, 38), radius=s(18), fill="#ffffff")
    d.rounded_rectangle(box(10, 6, 86, 16), radius=s(5), fill=(255, 255, 255, 255))
    return finish(img, w, h)


def trophy():
    w, h = 80, 80
    img, d = canvas(w, h)
    d.rounded_rectangle(box(24, 62, 56, 74), radius=s(3), fill="#a0612b", outline="#5c3417", width=s(3))
    d.rectangle(box(36, 46, 44, 62), fill="#f4c542", outline="#a77b12", width=s(2))
    d.arc(box(6, 12, 34, 42), 90, 270, fill="#a77b12", width=s(5))
    d.arc(box(46, 12, 74, 42), 270, 90, fill="#a77b12", width=s(5))
    d.pieslice(box(16, -6, 64, 52), 0, 180, fill="#f4c542", outline="#a77b12", width=s(4))
    d.line([s(18), s(20), s(62), s(20)], fill="#a77b12", width=s(4))
    return finish(img, w, h)


def wave_icon():
    w, h = 80, 80
    img, d = canvas(w, h)
    for y0, col in ((28, "#2ec4b6"), (52, "#abe3f5")):
        pts = [(s(8 + x), s(y0 + 8 * math.sin((x / 64) * 2 * math.pi))) for x in range(0, 65, 2)]
        d.line(pts, fill="#1b7f76", width=s(13), joint="curve")
        d.line(pts, fill=col, width=s(7), joint="curve")
    return finish(img, w, h)


def main(out):
    os.makedirs(out, exist_ok=True)
    sprites = {
        "surfer.png": surfer(),
        "shark.png": shark(),
        "chest.png": chest(),
        "coin.png": coin(),
        "hourglass.png": hourglass(),
        "buoy.png": buoy(),
        "shield.png": shield(),
        "arrow.png": arrow(),
        "foam.png": soft_circle(48, (255, 255, 255)),
        "glow.png": soft_circle(96, (255, 236, 150)),
        "wave_line.png": wave_line(),
        "ring.png": ring(),
        "ui_panel.png": panel(),
        "ui_button.png": button(),
        "ui_plaque.png": plaque(),
        "ui_bar_bg.png": bar_bg(),
        "ui_bar_fill.png": bar_fill(),
        "icon_trophy.png": trophy(),
        "icon_wave.png": wave_icon(),
    }
    for name, img in sprites.items():
        save(img, out, name)
    print("\n".join(f"{n} {i.size}" for n, i in sprites.items()))


if __name__ == "__main__":
    main(sys.argv[1])
