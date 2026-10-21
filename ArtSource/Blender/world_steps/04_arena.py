# World step 4 (route R4): the desert plateau arena. Fixed size (course L8 lesson 25): the top is
# faceted ground (constrained Delaunay over jittered points, ~2 m triangles, z ≈ 0) in sand and
# ochre patches; an irregular rim drops into deep jagged cliffs (palette row 5) that taper and fade
# into the background haze, a floating diorama as in the mood frame. Look pass 2026-10-21: the
# plateau is a rounded square for the diagonal perspective camera, cliffs go 12 m down.
import bpy, bmesh, math, random
from mathutils import Vector
from mathutils.geometry import delaunay_2d_cdt

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]
random.seed(21)

HALF_X = 15.5
HALF_Y = 14.0
CELL = 2.0
RIM = 96


def uv_of(col, row):
    return ((col + 0.5) / 8.0, 1 - (row + 0.5) / 8.0)


SHADES = [6, 5, 0, 1, 2, 3, 4, 7]  # palette row 4, darker to lighter sand


def noise(x, y):
    return (math.sin(x * 0.23 + 1.3) * math.cos(y * 0.29 - 0.4) + 0.6 * math.sin(x * 0.11 - y * 0.17 + 2.1) + 0.35 * math.sin(x * 0.53 + y * 0.47)) / 1.95


def rim_radius(angle):
    c, s = math.cos(angle), math.sin(angle)
    return 1.0 / ((abs(c) / HALF_X) ** 4 + (abs(s) / HALF_Y) ** 4) ** 0.25


rim = []
for i in range(RIM):
    a = 2 * math.pi * i / RIM
    r = rim_radius(a) * (1.0 + random.uniform(-0.035, 0.02))
    rim.append(Vector((math.cos(a) * r, math.sin(a) * r)))
points = list(rim)
y = -HALF_Y + CELL
while y < HALF_Y - 0.5:
    x = -HALF_X + CELL
    while x < HALF_X - 0.5:
        p = Vector((x + random.uniform(-0.6, 0.6), y + random.uniform(-0.6, 0.6)))
        if p.length < rim_radius(math.atan2(p.y, p.x)) * 0.92:
            points.append(p)
        x += CELL
    y += CELL

coords, _, faces, _, _, _ = delaunay_2d_cdt(points, [], [list(range(RIM))], 2, 1e-5)

bm = bmesh.new()
uv = bm.loops.layers.uv.verify()
verts = [bm.verts.new((c.x, c.y, 0.0 if i < RIM else random.uniform(-0.015, 0.015))) for i, c in enumerate(coords)]
for tri in faces:
    f = bm.faces.new([verts[i] for i in tri])
    if f.normal.z < 0:
        f.normal_flip()
    # calm sand: palette row 4 shades chosen by smooth noise over metres, a little per-face jitter
    c = f.calc_center_median()
    v = noise(c.x, c.y) * 0.5 + 0.5
    col = SHADES[min(7, max(0, int(v * 8 + ((len(bm.faces) - 1) % 3 - 1) * 0.35)))]
    for l in f.loops:
        l[uv].uv = uv_of(col, 4)

# cliffs: jagged bands tapering inward to a floating-island tip; palette row 5 goes from the lit
# terracotta rim through rust and dusty violet into the background haze. Each band is split into
# two triangles per quad with alternating diagonals, so the faces read as faceted rock columns.
BANDS = ((-1.0, 0.985, 0), (-2.7, 0.96, 1), (-4.8, 0.92, 2), (-7.2, 0.84, 3), (-9.6, 0.7, 4), (-11.6, 0.5, 5), (-12.8, 0.3, 6))
prev = verts[:RIM]
for depth, taper, colour in BANDS:
    ring = []
    for v in prev:
        k = taper + random.uniform(-0.02, 0.02)
        ring.append(bm.verts.new((v.co.x * k + random.uniform(-0.3, 0.3), v.co.y * k + random.uniform(-0.3, 0.3), depth + random.uniform(-0.45, 0.45))))
    for i in range(RIM):
        j = (i + 1) % RIM
        quads = ((prev[j], ring[j], ring[i]), (prev[j], ring[i], prev[i])) if i % 2 else ((prev[j], ring[j], prev[i]), (ring[j], ring[i], prev[i]))
        for tri in quads:
            f = bm.faces.new(tri)
            shade = min(7, colour + (1 if random.random() < 0.18 else 0))
            for l in f.loops:
                l[uv].uv = uv_of(shade, 5)
    prev = ring
# the tip: a fan to one point (a jittered 96-gon cap would self-intersect and be dropped on import)
tip = bm.verts.new((sum(v.co.x for v in prev) / RIM, sum(v.co.y for v in prev) / RIM, min(v.co.z for v in prev) - 0.6))
bottom = []
for i in range(RIM):
    f = bm.faces.new((prev[(i + 1) % RIM], prev[i], tip))
    for l in f.loops:
        l[uv].uv = uv_of(7, 5)
    bottom.append(f)
bm.normal_update()
# outward-facing cliffs, downward-facing tip
for f in bm.faces:
    c = f.calc_center_median()
    if f in bottom:
        if f.normal.z > 0:
            f.normal_flip()
    elif c.z < -0.05 and f.normal.dot(Vector((c.x, c.y, 0)).normalized()) < 0:
        f.normal_flip()

me = bpy.data.meshes.new("Arena")
bm.to_mesh(me)
bm.free()
me.materials.append(PAL)
me.materials.append(GLOW)
for poly in me.polygons:
    poly.use_smooth = False
old = bpy.data.objects.get("Arena")
if old:
    bpy.data.objects.remove(old, do_unlink=True)
arena = bpy.data.objects.new("Arena", me)
scene.collection.objects.link(arena)
print("arena tris", sum(len(p.vertices) - 2 for p in me.polygons), "points", len(points), "top tris", len(faces))
