# World step 3 (route R4): the mine — an octagonal disc with a raised cap, four glowing prongs,
# a pulsing orange light (own bone in step 6) and a flat trigger ring of radius 1 that
# Unity scales to MineConfig.TriggerRadius.
import bpy, bmesh, math
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


OFFSET = (4.5, 0, 0)  # authoring slot next to the dome; step 7 exports each asset at the origin

bm = bmesh.new()
disc = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=8, radius1=0.36, radius2=0.32, depth=0.09)["verts"]
bmesh.ops.rotate(bm, verts=disc, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(22.5), 3, "Z"))
bmesh.ops.translate(bm, vec=(0, 0, 0.045), verts=disc)
bm.normal_update()
paint(bm, bm.faces, 3, 2)
paint(bm, [f for f in bm.faces if f.normal.z > 0.9], 7, 2)
cap = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=8, radius1=0.22, radius2=0.13, depth=0.07)["verts"]
bmesh.ops.translate(bm, vec=(0, 0, 0.125), verts=cap)
paint(bm, list({f for v in cap for f in v.link_faces}), 2, 2)
for i in range(4):
    a = math.radians(45 + 90 * i)
    prong = bmesh.ops.create_cube(bm, size=1.0)["verts"]
    bmesh.ops.scale(bm, vec=(0.05, 0.05, 0.05), verts=prong)
    bmesh.ops.translate(bm, vec=(math.cos(a) * 0.27, math.sin(a) * 0.27, 0.105), verts=prong)
    paint(bm, list({f for v in prong for f in v.link_faces}), 6, 0, True)
to_object("Mine_Body", bm, OFFSET)

bm = bmesh.new()
bmesh.ops.create_icosphere(bm, subdivisions=1, radius=0.065)
paint(bm, bm.faces, 6, 3, True)
to_object("Mine_Light", bm, (OFFSET[0], OFFSET[1], 0.19))

bm = bmesh.new()
segments = 32
inner = []
outer = []
for i in range(segments):
    a = 2 * math.pi * i / segments
    inner.append(bm.verts.new((math.cos(a) * 0.94, math.sin(a) * 0.94, 0.012)))
    outer.append(bm.verts.new((math.cos(a) * 1.0, math.sin(a) * 1.0, 0.012)))
for i in range(segments):
    j = (i + 1) % segments
    bm.faces.new((inner[i], outer[i], outer[j], inner[j]))
paint(bm, bm.faces, 6, 0, True)
to_object("Mine_TriggerRing", bm, OFFSET)

tris = 0
for ob in bpy.data.objects:
    if ob.name.startswith("Mine_"):
        tris += sum(len(p.vertices) - 2 for p in ob.data.polygons)
print("mine built, tris", tris)
