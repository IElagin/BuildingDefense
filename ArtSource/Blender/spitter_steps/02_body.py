# Spitter step 2 (final assignment 2026-10-29): the bomber body as an acid spitter. Run after bomber_steps/01_scene.py,
# then bomber_steps 03 (limbs), 04 (rig), 05 (clips), spitter 05 (Spit clip) and 06 (export). Part names match the bomber
# rig. Differences: a rounder purple abdomen with an acid-green glowing sac on its back, a spout on the head.
import bpy, bmesh, math
from mathutils import Vector, Matrix

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]


def sw(col):
    return ((col + 0.5) / 8.0, 1 - 0.5 / 8.0)


def paint(bm, faces, col, glow):
    uv = bm.loops.layers.uv.verify()
    for f in faces:
        f.material_index = 1 if glow else 0
        for l in f.loops:
            l[uv].uv = sw(col)


def to_object(name, bm):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(PAL)
    me.materials.append(GLOW)
    for poly in me.polygons:
        poly.use_smooth = False
    ob = bpy.data.objects.new(name, me)
    scene.collection.objects.link(ob)
    return ob


# abdomen: faceted egg; upper faces clustered around seeds into plates, inset cracks glow, belly glows
bm = bmesh.new()
bmesh.ops.create_icosphere(bm, subdivisions=2, radius=1.0)
for v in bm.verts:
    x, y, z = v.co
    taper = 1.0 - 0.10 * max(0.0, y)
    v.co = Vector((x * 0.33 * taper, y * 0.36, z * 0.31 * taper))
bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(-14), 3, "X"))
bmesh.ops.translate(bm, vec=(0, -0.26, 0.37), verts=bm.verts)
bm.normal_update()
seeds = []
for sy in (0.02, -0.12, -0.27, -0.42):
    for sx in (-0.13, 0.13):
        seeds.append(Vector((sx, sy, 0.62)))
seeds.append(Vector((0.0, -0.58, 0.45)))
for sx in (-0.30, 0.30):
    seeds.append(Vector((sx, -0.20, 0.42)))
groups = {}
belly = []
for f in bm.faces:
    c = f.calc_center_median()
    if c.z < 0.27:
        belly.append(f)
        continue
    best = 0
    bd = 9.0
    for i, s in enumerate(seeds):
        d = (c - s).length
        if d < bd:
            bd = d
            best = i
    groups.setdefault(best, []).append(f)
plates = []
for k in groups:
    fs = groups[k]
    if len(fs) > 1:
        plates.extend(bmesh.ops.dissolve_faces(bm, faces=fs, use_verts=False)["region"])
    else:
        plates.extend(fs)
bm.normal_update()
before = set(bm.faces)
bmesh.ops.inset_individual(bm, faces=plates, thickness=0.012, depth=0.0)
cracks = [f for f in bm.faces if f not in before]
for f in plates:
    n = f.normal.copy()
    for v in f.verts:
        v.co += n * 0.014
paint(bm, plates, 3, False)
paint(bm, cracks, 5, True)
paint(bm, belly, 5, True)
# the acid sac: a faceted glowing bulb sitting in the plates at the back of the abdomen
sac = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)["verts"]
bmesh.ops.scale(bm, vec=(0.17, 0.2, 0.15), verts=sac)
bmesh.ops.translate(bm, vec=(0, -0.36, 0.62), verts=sac)
paint(bm, list({f for v in sac for f in v.link_faces}), 4, True)
to_object("Abdomen", bm)


def ring(name, centre, radii, tilt):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=7, v_segments=4, radius=1.0)
    bmesh.ops.scale(bm, vec=radii, verts=bm.verts)
    bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(tilt), 3, "X"))
    bmesh.ops.translate(bm, vec=centre, verts=bm.verts)
    bm.normal_update()
    top = [f for f in bm.faces if f.normal.z > 0.35]
    paint(bm, bm.faces, 1, False)
    paint(bm, top, 0, False)
    return to_object(name, bm)


ring("Thorax", Vector((0, 0.09, 0.30)), (0.135, 0.085, 0.115), -8)
ring("Neck", Vector((0, 0.18, 0.28)), (0.11, 0.065, 0.095), -12)

# head: dark wedge plus two small faceted orange eyes
bm = bmesh.new()
bmesh.ops.create_uvsphere(bm, u_segments=6, v_segments=4, radius=1.0)
for v in bm.verts:
    x, y, z = v.co
    v.co = Vector((x * (0.115 - 0.03 * max(0.0, y)), y * 0.095, z * 0.08))
bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(18), 3, "X"))
bmesh.ops.translate(bm, vec=(0, 0.27, 0.255), verts=bm.verts)
paint(bm, bm.faces, 1, False)
for sx in (-1, 1):
    verts = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=0.022)["verts"]
    bmesh.ops.translate(bm, vec=(0.05 * sx, 0.335, 0.275), verts=verts)
    paint(bm, list({f for v in verts for f in v.link_faces}), 6, True)
# spout: a short dark tube between the mandibles with a glowing acid lip
spout = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=0.035, radius2=0.028, depth=0.11)["verts"]
bmesh.ops.rotate(bm, verts=spout, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(-80), 3, "X"))
bmesh.ops.translate(bm, vec=(0, 0.36, 0.245), verts=spout)
spout_faces = list({f for v in spout for f in v.link_faces})
paint(bm, spout_faces, 1, False)
paint(bm, [f for f in spout_faces if f.calc_center_median().y > 0.405], 4, True)
to_object("Head", bm)
print("spitter body built")
