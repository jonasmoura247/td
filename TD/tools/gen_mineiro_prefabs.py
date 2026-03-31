# -*- coding: utf-8 -*-
from pathlib import Path

BASE = Path(r"e:\github\td\TD\Assets\Prefabs\Characters\Humanos\HumanoMineiro.prefab").read_text(encoding="utf-8")

HUMAN_RACIAL_TAIL = "  multiplicadorDanoRacial: 1\n  tempoInvulnerabilidade: 0"

SPECS = [
    (
        Path(r"e:\github\td\TD\Assets\Sprites\Racas\Elfos\ElfoMineiro.prefab"),
        {
            "go": "8100100100100100",
            "tr": "8100100100100101",
            "sr": "8100100100100102",
            "rb": "8100100100100103",
            "pl": "8100100100100104",
            "sc": "8100100100100105",
            "mv": "8100100100100106",
            "name": "ElfoMineiro",
            "sprite": "m_Sprite: {fileID: -3230563305799599193, guid: 0681ff10aa52ecc4f9dcbde0c3eebe61, type: 3}",
            "script_guid": "d2c3b4a5968778695a4b3c2d1e0f1a2b",
            "class_name": "ElfoMineiroController",
            "racial_tail": "  multiplicadorVelocidade: 1.1\n  multiplicadorDanoMagico: 1",
        },
    ),
    (
        Path(r"e:\github\td\TD\Assets\Sprites\Racas\Goblins\GoblinMineiro.prefab"),
        {
            "go": "8200200200200200",
            "tr": "8200200200200201",
            "sr": "8200200200200202",
            "rb": "8200200200200203",
            "pl": "8200200200200204",
            "sc": "8200200200200205",
            "mv": "8200200200200206",
            "name": "GoblinMineiro",
            "sprite": "m_Sprite: {fileID: -4668589943839710107, guid: b334f151d5a19b14db13bbefe464dd50, type: 3}",
            "script_guid": "e1d2c3b4a5968778695a4b3c2d1e0f1a",
            "class_name": "GoblinMineiroController",
            "racial_tail": "  multiplicadorDanoRacial: 1",
        },
    ),
    (
        Path(r"e:\github\td\TD\Assets\Sprites\Racas\Anoes\AnaoMineiro.prefab"),
        {
            "go": "8300300300300300",
            "tr": "8300300300300301",
            "sr": "8300300300300302",
            "rb": "8300300300300303",
            "pl": "8300300300300304",
            "sc": "8300300300300305",
            "mv": "8300300300300306",
            "name": "AnaoMineiro",
            "sprite": "m_Sprite: {fileID: 801292187771708722, guid: 1e23d33e3e44d0a43a7fc65808d85784, type: 3}",
            "script_guid": "c3b4a5968778695a4b3c2d1e0f1a2b3c",
            "class_name": "AnaoMineiroController",
            "racial_tail": "  multiplicadorDanoRacial: 1\n  multiplicadorVelocidade: 0.88",
        },
    ),
]

OLD_SPRITE = "m_Sprite: {fileID: 410819640858332543, guid: b61d1ea5d6b475444bf7244e38c4857c, type: 3}"

for out_path, s in SPECS:
    c = BASE
    c = c.replace(HUMAN_RACIAL_TAIL, s["racial_tail"])
    c = c.replace("2579156320764190982", s["go"])
    c = c.replace("2091335151862666265", s["tr"])
    c = c.replace("8842560767674354218", s["sr"])
    c = c.replace("2661450170964638028", s["rb"])
    c = c.replace("1872294984200076049", s["pl"])
    c = c.replace("9107515997009673633", s["sc"])
    c = c.replace("5531135507202679762", s["mv"])
    c = c.replace("m_Name: HumanoMineiro", "m_Name: " + s["name"])
    c = c.replace(OLD_SPRITE, s["sprite"])
    c = c.replace("guid: c90340d45cc713c4d9e52e1bec61b2e2", "guid: " + s["script_guid"])
    c = c.replace("Assembly-CSharp::HumanoMineiroController", "Assembly-CSharp::" + s["class_name"])
    out_path.write_text(c, encoding="utf-8")
    print("Wrote", out_path)
