"""Rebuild the AUT0-CH mark and wordmark as clean SVG (text converted to paths)."""
import math
from fontTools.ttLib import TTFont
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.boundsPen import BoundsPen
from fontTools.pens.transformPen import TransformPen

NAVY, TEAL = "#1D3557", "#2A9D8F"
FONT = TTFont("node_modules/@fontsource/montserrat/files/montserrat-latin-800-normal.woff")
GS, CMAP, UPM = FONT.getGlyphSet(), FONT.getBestCmap(), FONT["head"].unitsPerEm
CAP = FONT["OS/2"].sCapHeight


def hexagon(cx, cy, R):
    """Flat-top regular hexagon points."""
    return " ".join(f"{cx + R*math.cos(math.radians(a)):.2f},{cy + R*math.sin(math.radians(a)):.2f}"
                    for a in (0, 60, 120, 180, 240, 300))


def mark(cx, cy, R, uid):
    """Hex nut with round hole and a teal slash clipped to the hole (slashed zero)."""
    r = R * 0.53
    w = R * 0.10
    d = r * 1.2
    return f'''<defs><clipPath id="hole{uid}"><circle cx="{cx}" cy="{cy}" r="{r:.2f}"/></clipPath></defs>
  <path fill="{NAVY}" fill-rule="evenodd" d="M{hexagon(cx, cy, R).replace(' ', ' L')} Z M{cx - r:.2f},{cy} a{r:.2f},{r:.2f} 0 1,0 {2*r:.2f},0 a{r:.2f},{r:.2f} 0 1,0 {-2*r:.2f},0 Z"/>
  <line clip-path="url(#hole{uid})" x1="{cx - d:.2f}" y1="{cy + d:.2f}" x2="{cx + d:.2f}" y2="{cy - d:.2f}" stroke="{TEAL}" stroke-width="{w:.2f}"/>'''


def glyph_path(ch, x, baseline, scale):
    """Return (svg path d, advance) for one character, y-flipped onto the baseline."""
    name = CMAP[ord(ch)]
    pen = SVGPathPen(GS)
    GS[name].draw(TransformPen(pen, (scale, 0, 0, -scale, x, baseline)))
    return pen.getCommands(), GS[name].width * scale


def zero(x, baseline, cap_h, width):
    """Teal slashed zero: rounded stadium with a stadium counter and a teal slash."""
    top, h = baseline - cap_h, cap_h
    stroke = width * 0.29
    rx = width / 2
    ix, iw = x + stroke, width - 2 * stroke
    itop, ih = top + stroke * 0.80, h - 2 * stroke * 0.80
    irx = iw / 2

    def stadium(sx, sy, sw, sh, r):
        # rounded rect as a path so the counter can be cut out with evenodd (transparent hole)
        return (f"M{sx + r:.2f},{sy:.2f} H{sx + sw - r:.2f} A{r:.2f},{r:.2f} 0 0 1 {sx + sw:.2f},{sy + r:.2f} "
                f"V{sy + sh - r:.2f} A{r:.2f},{r:.2f} 0 0 1 {sx + sw - r:.2f},{sy + sh:.2f} H{sx + r:.2f} "
                f"A{r:.2f},{r:.2f} 0 0 1 {sx:.2f},{sy + sh - r:.2f} V{sy + r:.2f} A{r:.2f},{r:.2f} 0 0 1 {sx + r:.2f},{sy:.2f} Z")

    inner = stadium(ix, itop, iw, ih, irx)
    body = f'<path fill="{TEAL}" fill-rule="evenodd" d="{stadium(x, top, width, h, rx)} {inner}"/>'
    cx, cy = x + width / 2, top + h / 2
    slash = (f'<line x1="{cx - iw:.2f}" y1="{cy + iw*1.15:.2f}" x2="{cx + iw:.2f}" y2="{cy - iw*1.15:.2f}" '
             f'stroke="{TEAL}" stroke-width="{stroke*0.62:.2f}" clip-path="url(#zc)"/>')
    clip = f'<defs><clipPath id="zc"><path d="{inner}"/></clipPath></defs>'
    return clip + body + slash


def wordmark_svg(knockout_bg=None):
    """Horizontal lockup: mark + AUT0-CH. knockout_bg fills the zero's counter (white by default)."""
    cap_h = 64.0
    scale = cap_h / CAP
    baseline = 82.0
    R = 50.0
    parts = [mark(R + 4, 50, R, "w")]
    x = 2 * R + 4 + 30
    tracking = 4.0
    paths = []
    for ch in "AUT":
        d, adv = glyph_path(ch, x, baseline, scale)
        paths.append(d)
        x += adv + tracking
    zw = cap_h * 0.80
    zero_svg = zero(x + 2, baseline, cap_h, zw)
    x += zw + 4 + tracking + 2
    for ch in "-CH":
        d, adv = glyph_path(ch, x, baseline, scale)
        paths.append(d)
        x += adv + tracking
    width = x - tracking + 4
    body = "\n  ".join(parts) + f'\n  <path fill="{NAVY}" d="{" ".join(paths)}"/>\n  ' + zero_svg
    if knockout_bg:
        body = body.replace('fill="#fff"', f'fill="{knockout_bg}"')
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width:.0f} 100" width="{width:.0f}" height="100">\n'
            f'  <title>AUT0-CH</title>\n  {body}\n</svg>\n')


def mark_svg(bg=None, pad=0.06):
    R = 50 * (1 - pad)
    rect = f'<rect width="100" height="100" rx="18" fill="{bg}"/>' if bg else ""
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100" height="100">\n'
            f'  <title>AUT0-CH</title>\n  {rect}\n  {mark(50, 50, R, "m")}\n</svg>\n')


if __name__ == "__main__":
    open("aut0-ch-mark.svg", "w").write(mark_svg())
    open("aut0-ch-mark-tile.svg", "w").write(mark_svg(bg="#ffffff", pad=0.16))
    open("aut0-ch-wordmark.svg", "w").write(wordmark_svg())
    print("ok")
