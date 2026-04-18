import re, os

BASE = r"C:\Users\Jonas\Desktop\TD\td\TD\Assets\Sprites\Npc"
OLD_GUID = "a0e1abedde25ba54bb0f2ccaa43c8029"

MOBS = [
    ("Amazona",   "MobAmazon.prefab",    "MobAmazona",   "b556d3999f3c5884290df7d98413598c"),
    ("Aranha",    "MobAranha.prefab",    "MobAranha",    "dc1a506cc4c3a7643888231acf93c39a"),
    ("Espantalho","MobEspantalho.prefab","MobEspantalho","7355307733ee3564886bc63301e9824e"),
    ("Esqueleto", "MobEsqueleto.prefab", "MobEsqueleto", "51569a2275819f9408b805dc3652256a"),
    ("Formiga",   "MobFormiga.prefab",   "MobFormiga",   "149880eb234e5384f9fb0b9c6a56c63c"),
    ("GolemDePedra","MobGolemPedra.prefab","MobGolemPedra","4b42c08d80e1ede4a8924c53694c8d32"),
    ("Harpia",    "MobHarpia.prefab",    "MobHarpia",    "8a93f5a6a4d853e469518fe496de52d1"),
    ("Lobo",      "MobLobo.prefab",      "MobLobo",      "be78824f036e4844bad308f33cb16d54"),
    ("Minotauro", "MobMinotauro.prefab", "MobMinotauro", "05267f55be2bc7242aa9d270dd9d6961"),
    ("Ogro",      "MobOgre.prefab",      "MobOgre",      "4bb615c0ebc25c847b4a124f0fa70eb0"),
    ("Orc",       "MobOrc.prefab",       "MobOrc",       "5c7dcdd93a6f44545a57eda30116aa30"),
    ("Rato",      "MobRato.prefab",      "MobRato",      "ec3b5ec290530d44f93e310b656834c4"),
    ("Tigre",     "MobTigre.prefab",     "MobTigre",     "c7af7632803c5f74ab467a929b744f5d"),
    ("Zumbie",    "MobZumbie.prefab",    "MobZumbie",    "a60f8fc2a5d0807459537edef4aebc43"),
]

for folder, filename, classname, new_guid in MOBS:
    path = os.path.join(BASE, folder, filename)
    if not os.path.exists(path):
        print(f"NOT FOUND: {path}")
        continue

    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Replace GoblinGuerreiro guid with mob-specific guid
    content = content.replace(
        f"guid: {OLD_GUID}",
        f"guid: {new_guid}"
    )

    # 2. Replace class identifier
    content = content.replace(
        "Assembly-CSharp::GoblinGuerreiro",
        f"Assembly-CSharp::{classname}"
    )

    # 3. Remove multiplicadorDanoRacial line
    content = re.sub(r"  multiplicadorDanoRacial: \S+\n", "", content)

    # 4. Replace multiplicadorDanoGuerreiro with multiplicadorDano
    content = re.sub(r"  multiplicadorDanoGuerreiro: \S+", "  multiplicadorDano: 1", content)

    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)

    print(f"OK: {classname} -> {filename}")

print("Done.")
