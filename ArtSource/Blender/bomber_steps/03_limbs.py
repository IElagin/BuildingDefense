# Step 3: two-segment mandibles and six three-segment legs as separate rigid parts.
# JOINTS is shared with step 4 (bones are placed on the same points).
import bpy, bmesh
from mathutils import Vector

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]
LEGS = (("1", 0.19, 0.32, 0.47, 0.53), ("2", 0.12, 0.12, 0.13, 0.12), ("3", 0.05, -0.12, -0.32, -0.40))


def segment(name, a, b, r1, r2, col):
    L = (b - a).length
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=4, radius1=r1, radius2=r2, depth=L)
    bmesh.ops.translate(bm, vec=(0, 0, L / 2), verts=bm.verts)
    q = Vector((0, 0, 1)).rotation_difference((b - a).normalized())
    bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=q.to_matrix())
    bmesh.ops.translate(bm, vec=a, verts=bm.verts)
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        for l in f.loops:
            l[uv].uv = ((col + 0.5) / 8.0, 1 - 0.5 / 8.0)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(PAL)
    me.materials.append(GLOW)
    for poly in me.polygons:
        poly.use_smooth = False
    ob = bpy.data.objects.new(name, me)
    scene.collection.objects.link(ob)


for sx, s in ((-1, "L"), (1, "R")):
    base = Vector((0.04 * sx, 0.335, 0.225))
    mid = Vector((0.07 * sx, 0.41, 0.205))
    tip = Vector((0.018 * sx, 0.465, 0.19))
    segment("Mandible_%s_1" % s, base, mid, 0.017, 0.012, 2)
    segment("Mandible_%s_2" % s, mid, tip, 0.012, 0.003, 2)
    for idx, hy, ky, ay, fy in LEGS:
        hip = Vector((0.07 * sx, hy, 0.24))
        knee = Vector((0.30 * sx, ky, 0.52))
        ankle = Vector((0.47 * sx, ay, 0.20))
        foot = Vector((0.53 * sx, fy, 0.0))
        segment("Leg_%s%s_1" % (s, idx), hip, knee, 0.032, 0.024, 0)
        segment("Leg_%s%s_2" % (s, idx), knee, ankle, 0.024, 0.016, 0)
        segment("Leg_%s%s_3" % (s, idx), ankle, foot, 0.016, 0.005, 0)
print("limbs built")
