# World step 8 (route R4, look pass 2026-10-21): the hive burrow, a crater like in the mood frame.
# A raised sand rim around a dark throat; everything stays above the ground (the arena top at z = 0
# would hide a real pit), the dark centre reads as depth. Three pebbles on the rim. ~180 triangles,
# palette rows 4 (sand), 1 (dark sand, rocks) and 0/3 (dark throat). Placed beside the props for review.
import bpy, bmesh, math, random
from mathutils import Vector

random.seed(8)
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]
SEGMENTS = 14


def uv_of(col, row):
    return ((col + 0.5) / 8.0, 1 - (row + 0.5) / 8.0)


# (radius, height, colour of the band from the previous ring to this one)
RINGS = (
    (1.95, 0.0, None),
    (1.4, 0.36, (7, 4)),    # outer slope, light sand
    (1.1, 0.22, (1, 4)),    # crest to inner lip, sand
    (0.85, 0.1, (1, 1)),    # inner slope, dark sand
    (0.6, 0.06, (0, 3)),    # throat, dark brown
)

bm = bmesh.new()
uv = bm.loops.layers.uv.verify()
rings = []
for radius, height, _ in RINGS:
    ring = []
    for i in range(SEGMENTS):
        a = 2 * math.pi * (i + random.uniform(-0.18, 0.18)) / SEGMENTS
        r = radius * (1 + random.uniform(-0.08, 0.08))
        z = height * (1 + random.uniform(-0.2, 0.2)) if height > 0 else 0.0
        ring.append(bm.verts.new((math.cos(a) * r, math.sin(a) * r, z)))
    rings.append(ring)

for k in range(1, len(rings)):
    colour = RINGS[k][2]
    outer, inner = rings[k - 1], rings[k]
    for i in range(SEGMENTS):
        j = (i + 1) % SEGMENTS
        for tri in ((outer[i], outer[j], inner[j]), (outer[i], inner[j], inner[i])):
            f = bm.faces.new(tri)
            for l in f.loops:
                l[uv].uv = uv_of(*colour)

centre = bm.verts.new((0, 0, 0.04))
for i in range(SEGMENTS):
    j = (i + 1) % SEGMENTS
    f = bm.faces.new((rings[-1][i], rings[-1][j], centre))
    for l in f.loops:
        l[uv].uv = uv_of(0, 0)  # chitin black: the hole

for n in range(3):
    a = 2 * math.pi * (n / 3 + random.uniform(-0.08, 0.08))
    r = 1.45 + random.uniform(-0.1, 0.25)
    size = random.uniform(0.12, 0.2)
    pebble = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=size)["verts"]
    for v in pebble:
        v.co.x = v.co.x * random.uniform(0.8, 1.3) + math.cos(a) * r
        v.co.y = v.co.y * random.uniform(0.8, 1.3) + math.sin(a) * r
        v.co.z = v.co.z * 0.7 + 0.22
    for f in {f for v in pebble for f in v.link_faces}:
        for l in f.loops:
            l[uv].uv = uv_of(2 if n % 2 else 3, 1)

bm.normal_update()
for f in bm.faces:
    if f.normal.z < -0.2:
        f.normal_flip()

me = bpy.data.meshes.new("Burrow")
bm.to_mesh(me)
bm.free()
me.materials.append(PAL)
me.materials.append(GLOW)
for poly in me.polygons:
    poly.use_smooth = False
old = bpy.data.objects.get("Burrow")
if old:
    bpy.data.objects.remove(old, do_unlink=True)
burrow = bpy.data.objects.new("Burrow", me)
bpy.context.scene.collection.objects.link(burrow)
burrow.location = (8.0, 18.0, 0.0)
print("burrow tris", sum(len(p.vertices) - 2 for p in me.polygons))
