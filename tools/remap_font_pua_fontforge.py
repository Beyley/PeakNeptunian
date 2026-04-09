#!/usr/bin/env fontforge

import sys

import fontforge


SOURCE_START = 0xF1E00
SOURCE_END = 0xF1EFF
TARGET_START = 0xE000


def remap_codepoint(codepoint):
    return TARGET_START + (codepoint - SOURCE_START)


def main(argv):
    if len(argv) != 3:
        raise SystemExit(
            "usage: fontforge -script tools/remap_font_pua_fontforge.py input-font output-font"
        )

    input_font, output_font = argv[1], argv[2]
    font = fontforge.open(input_font)

    by_unicode = {}
    for glyph in font.glyphs():
        if glyph.unicode >= 0:
            by_unicode[glyph.unicode] = glyph

    remaps = []
    for codepoint in range(SOURCE_START, SOURCE_END + 1):
        glyph = by_unicode.get(codepoint)
        if glyph is None:
            continue

        target = remap_codepoint(codepoint)
        conflict = by_unicode.get(target)
        if conflict is not None and conflict.glyphname != glyph.glyphname:
            raise SystemExit(
                f"refusing to overwrite U+{target:04X}: {conflict.glyphname} != {glyph.glyphname}"
            )

        remaps.append((glyph, target))

    if not remaps:
        raise SystemExit("no codepoints found in U+F1E00..U+F1EFF")

    for glyph, _ in remaps:
        glyph.unicode = -1

    for glyph, target in remaps:
        glyph.unicode = target

    font.generate(output_font)
    print(f"Remapped {len(remaps)} glyphs to U+E000..U+E0FF")


if __name__ == "__main__":
    main(sys.argv)