# World step 5 (route R4): arena props. Sandstone rocks (displaced icosahedra, lit top faces lighter)
# and glowing blue crystal clusters (hexagonal prisms with pointed tips on a dark base).
# Each prop is a separate object with its origin on the ground; step 7 exports one FBX per prop.
import bpy, bmesh, math, random
from mathutils import Vector, Matrix

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]
random.seed(5)


def uv_of(col, row):
    return ((col + 0.5) / 8.0, 1 - (row + 0.5) / 8.0)


def paint(bm, faces, col, row, glow=False):
    uv = bm.loops.layers.uv.verify()
    for f in faces:
        f.material_index = 1 if glow else 0
        for l in f.loops:
            l[uv].uv = uv_of(col, row)


def to_object(name, bm, location):
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


def boulder(bm, size, centre, squash):
    verts = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)["verts"]
    for v in verts:
        v.co *= 1.0 + random.uniform(-0.22, 0.18)
        v.co = Vector((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2] * squash))
    bmesh.ops.translate(bm, vec=centre, verts=verts)
    faces = list({f for v in verts for f in v.link_faces})
    bm.normal_update()
    for f in faces:
        nz = f.normal.z
        col = (7, 4) if nz > 0.6 else ((1, 1) if nz > -0.1 else (0, 1))
        paint(bm, [f], *col)
    # sink the bottom into the ground
    low = min(v.co.z for v in verts)
    for v in verts:
        v.co.z -= low + 0.12 * size[2]


ROCKS = (
    ("Rock_A", ((1.0, 0.8, 0.75), (0.75, 0.6, 0.55)), (8, 3)),
    ("Rock_B", ((0.65, 0.55, 0.55),), (8, 6)),
    ("Rock_C", ((0.32, 0.28, 0.25), (0.22, 0.2, 0.2), (0.18, 0.15, 0.16)), (8, 9)),
)
for name, sizes, slot in ROCKS:
    bm = bmesh.new()
    for i, size in enumerate(sizes):
        offset = (0.0, 0.0, 0.0) if i == 0 else (random.uniform(-0.9, 0.9) * sizes[0][0], random.uniform(-0.6, 0.6) * sizes[0][1], 0.0)
        boulder(bm, size, offset, 0.85)
    to_object(name, bm, (slot[0], slot[1], 0))


def crystal(bm, base, height, radius, tilt, yaw):
    prism = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=radius, radius2=radius * 0.9, depth=height)["verts"]
    bmesh.ops.translate(bm, vec=(0, 0, height / 2), verts=prism)
    tip = bmesh.ops.create_cone(bm, cap_ends=False, segments=6, radius1=radius * 0.9, radius2=0.0, depth=radius * 1.6)["verts"]
    bmesh.ops.translate(bm, vec=(0, 0, height + radius * 0.8), verts=tip)
    verts = prism + tip
    rot = Matrix.Rotation(yaw, 3, "Z") @ Matrix.Rotation(tilt, 3, "X")
    bmesh.ops.rotate(bm, verts=verts, cent=(0, 0, 0), matrix=rot)
    bmesh.ops.translate(bm, vec=base, verts=verts)
    bm.normal_update()
    prism_faces = list({f for v in prism for f in v.link_faces})
    tip_faces = list({f for v in tip for f in v.link_faces})
    paint(bm, prism_faces, 3, 3, True)
    paint(bm, tip_faces, 4, 3, True)
    for f in prism_faces:
        if f.normal.dot(Vector((0.5, -0.7, 0.5))) < 0:
            paint(bm, [f], 7, 2)


CRYSTALS = (
    ("Crystal_A", 5, 1.0, (8, 12)),
    ("Crystal_B", 3, 0.65, (8, 15)),
)
for name, count, scale, slot in CRYSTALS:
    bm = bmesh.new()
    base = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)["verts"]
    for v in base:
        v.co = Vector((v.co.x * 0.42 * scale, v.co.y * 0.38 * scale, v.co.z * 0.16 * scale))
    paint(bm, list({f for v in base for f in v.link_faces}), 0, 3)
    for i in range(count):
        a = 2 * math.pi * i / count + random.uniform(-0.3, 0.3)
        d = 0.0 if i == 0 else random.uniform(0.12, 0.26) * scale
        h = (0.75 if i == 0 else random.uniform(0.3, 0.55)) * scale
        r = (0.11 if i == 0 else random.uniform(0.06, 0.09)) * scale
        tilt = 0.0 if i == 0 else random.uniform(0.25, 0.5)
        crystal(bm, (math.cos(a) * d, math.sin(a) * d, 0.0), h, r, tilt, a - math.pi / 2)
    to_object(name, bm, (slot[0], slot[1], 0))

tris = {}
for ob in bpy.data.objects:
    if ob.name.startswith(("Rock_", "Crystal_")):
        tris[ob.name] = sum(len(p.vertices) - 2 for p in ob.data.polygons)
print("props", tris)
