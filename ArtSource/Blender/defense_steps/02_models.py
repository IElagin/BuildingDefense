# Defense step 2 (route R4, final assignment 2026-10-29): the turret, the puddle and the plasma bolt in the game palette.
# Run after bomber_steps/01_scene.py in an untitled session (bpy.ops.wm.read_homefile(use_empty=True) first).
#   Turret_Base — a still octagonal plinth with teal lights; Turret_Head — the part that turns: pivot on the ground at
#   the turret axis, a yoke, a armoured head and twin barrels along +Y (Unity forward) at bolt height 0.45 m.
#   Puddle — a flat blob of radius 1 that Unity scales to PuddleConfig.Radius: dark teal rim, teal pool, glowing glints.
#   PlasmaBolt — a stretched glowing drop, 0.36 m long, pointing along +Y.
import bpy, bmesh, math, random
from mathutils import Matrix

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]


def uv_of(col, row):
    return ((col + 0.5) / 8.0, 1 - (row + 0.5) / 8.0)


def paint(bm, faces, col, row, glow=False):
    uv = bm.loops.layers.uv.verify()
    for f in faces:
        f.material_index = 1 if glow else 0
        for l in f.loops:
            l[uv].uv = uv_of(col, row)


def faces_of(verts):
    return list({f for v in verts for f in v.link_faces})


def to_object(name, bm, location=(0, 0, 0)):
    old = bpy.data.objects.get(name)
    if old:
        m = old.data
        bpy.data.objects.remove(old, do_unlink=True)
        if m.users == 0:
            bpy.data.meshes.remove(m)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(PAL)
    me.materials.append(GLOW)
    for poly in me.polygons:
        poly.use_smooth = False
    ob = bpy.data.objects.new(name, me)
    ob.location = location
    scene.collection.objects.link(ob)
    return ob


def cylinder(bm, segments, r1, r2, depth, z, rotate=0.0):
    verts = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments, radius1=r1, radius2=r2, depth=depth)["verts"]
    if rotate:
        bmesh.ops.rotate(bm, verts=verts, cent=(0, 0, 0), matrix=Matrix.Rotation(rotate, 3, "Z"))
    bmesh.ops.translate(bm, vec=(0, 0, z + depth / 2), verts=verts)
    return verts


def box(bm, size, center):
    verts = bmesh.ops.create_cube(bm, size=1.0)["verts"]
    bmesh.ops.scale(bm, vec=size, verts=verts)
    bmesh.ops.translate(bm, vec=center, verts=verts)
    return verts


TURRET = (-3.0, 0, 0)  # authoring slots side by side; the export puts each asset at the origin
PUDDLE = (0, 0, 0)
BOLT = (2.6, 0, 0.45)
OCT = math.radians(22.5)

# --- turret base: plinth, teal light band, top collar
bm = bmesh.new()
plinth = cylinder(bm, 8, 0.46, 0.4, 0.16, 0.0, OCT)
paint(bm, faces_of(plinth), 3, 2)
band = cylinder(bm, 8, 0.405, 0.405, 0.04, 0.16, OCT)
paint(bm, faces_of(band), 4, 2, True)
collar = cylinder(bm, 8, 0.36, 0.3, 0.06, 0.2, OCT)
paint(bm, faces_of(collar), 2, 2)
for i in range(4):
    a = math.radians(45 + 90 * i)
    foot = box(bm, (0.16, 0.16, 0.1), (math.cos(a) * 0.44, math.sin(a) * 0.44, 0.05))
    bmesh.ops.rotate(bm, verts=foot, cent=(math.cos(a) * 0.44, math.sin(a) * 0.44, 0), matrix=Matrix.Rotation(a, 3, "Z"))
    paint(bm, faces_of(foot), 3, 2)
to_object("Turret_Base", bm, TURRET)

# --- turret head (turns): yoke, armoured body, twin barrels with glowing muzzles, sensor eye, back fins
bm = bmesh.new()
neck = cylinder(bm, 8, 0.2, 0.17, 0.14, 0.24, OCT)
paint(bm, faces_of(neck), 2, 2)
body = box(bm, (0.42, 0.46, 0.24), (0, -0.02, 0.48))
# bevel the front top edge into a sloped armour plate
for v in body:
    if v.co.y > 0 and v.co.z > 0.5:
        v.co.y -= 0.12
        v.co.z -= 0.02
bm.normal_update()
paint(bm, faces_of(body), 1, 2)
paint(bm, [f for f in faces_of(body) if f.normal.z > 0.5], 0, 2)
for side in (-1, 1):
    cheek = box(bm, (0.08, 0.36, 0.18), (side * 0.25, -0.04, 0.46))
    paint(bm, faces_of(cheek), 3, 2)
    barrel = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=0.045, radius2=0.04, depth=0.5)["verts"]
    bmesh.ops.rotate(bm, verts=barrel, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(-90), 3, "X"))
    bmesh.ops.translate(bm, vec=(side * 0.1, 0.42, 0.45), verts=barrel)
    paint(bm, faces_of(barrel), 3, 2)
    muzzle = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=0.055, radius2=0.055, depth=0.06)["verts"]
    bmesh.ops.rotate(bm, verts=muzzle, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(-90), 3, "X"))
    bmesh.ops.translate(bm, vec=(side * 0.1, 0.66, 0.45), verts=muzzle)
    paint(bm, faces_of(muzzle), 6, 2, True)
eye = box(bm, (0.12, 0.04, 0.06), (0, 0.16, 0.56))
paint(bm, faces_of(eye), 4, 2, True)
for side in (-1, 1):
    fin = box(bm, (0.03, 0.16, 0.14), (side * 0.12, -0.3, 0.64))
    paint(bm, faces_of(fin), 3, 2)
to_object("Turret_Head", bm, TURRET)

# --- puddle: jittered blob, rim ring + inner pool + glints, a few mm above the ground
rng = random.Random(29)
bm = bmesh.new()
segments = 20
radii = [0.92 + rng.uniform(-0.06, 0.08) for _ in range(segments)]
centre = bm.verts.new((0, 0, 0.022))
rim_out, rim_in = [], []
for i in range(segments):
    a = 2 * math.pi * i / segments
    r = radii[i]
    rim_out.append(bm.verts.new((math.cos(a) * r, math.sin(a) * r, 0.012)))
    rim_in.append(bm.verts.new((math.cos(a) * r * 0.86, math.sin(a) * r * 0.86, 0.02)))
rim_faces, pool_faces = [], []
for i in range(segments):
    j = (i + 1) % segments
    rim_faces.append(bm.faces.new((rim_out[i], rim_out[j], rim_in[j], rim_in[i])))
    pool_faces.append(bm.faces.new((rim_in[i], rim_in[j], centre)))
paint(bm, rim_faces, 5, 2)
paint(bm, pool_faces, 4, 2)
for k in range(5):
    a = rng.uniform(0, 2 * math.pi)
    d = rng.uniform(0.2, 0.6)
    s = rng.uniform(0.05, 0.09)
    cx, cy = math.cos(a) * d, math.sin(a) * d
    glint = [bm.verts.new((cx + math.cos(t) * s, cy + math.sin(t) * s * 0.6, 0.026)) for t in (0, 2.1, 4.2)]
    paint(bm, [bm.faces.new(glint)], 6, 2, True)
to_object("Puddle", bm, PUDDLE)

# --- plasma bolt
bm = bmesh.new()
drop = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=0.1)["verts"]
bmesh.ops.scale(bm, vec=(1, 1.8, 1), verts=drop)
paint(bm, bm.faces, 6, 2, True)
to_object("PlasmaBolt", bm, BOLT)

tris = {}
for name in ("Turret_Base", "Turret_Head", "Puddle", "PlasmaBolt"):
    tris[name] = sum(len(p.vertices) - 2 for p in bpy.data.objects[name].data.polygons)
print("defense models built, tris", tris)
