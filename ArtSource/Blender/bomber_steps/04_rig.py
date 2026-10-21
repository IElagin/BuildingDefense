# Step 4: armature "Bomber"; every part follows one bone (bone parenting, no skin weights),
# so Unity imports plain MeshRenderers under the bone transforms.
import bpy
from mathutils import Vector

scene = bpy.context.scene
LEGS = (("1", 0.19, 0.32, 0.47, 0.53), ("2", 0.12, 0.12, 0.13, 0.12), ("3", 0.05, -0.12, -0.32, -0.40))
arm_data = bpy.data.armatures.new("BomberRig")
arm = bpy.data.objects.new("Bomber", arm_data)
scene.collection.objects.link(arm)
arm.show_in_front = True
for o in bpy.context.view_layer.objects:
    o.select_set(False)
bpy.context.view_layer.objects.active = arm
arm.select_set(True)
bpy.ops.object.mode_set(mode="EDIT")
eb = arm_data.edit_bones


def bone(name, head, tail, parent=None, connect=False):
    b = eb.new(name)
    b.head = Vector(head)
    b.tail = Vector(tail)
    if parent:
        b.parent = eb[parent]
        b.use_connect = connect


bone("Root", (0, 0, 0), (0, 0.25, 0))
bone("Body", (0, 0.0, 0.30), (0, 0.2, 0.30), "Root")
bone("Abdomen", (0, 0.02, 0.33), (0, -0.55, 0.42), "Body")
bone("Head", (0, 0.22, 0.26), (0, 0.34, 0.26), "Body")
for sx, s in ((-1, "L"), (1, "R")):
    bone("Mandible_%s_1" % s, (0.04 * sx, 0.335, 0.225), (0.07 * sx, 0.41, 0.205), "Head")
    bone("Mandible_%s_2" % s, (0.07 * sx, 0.41, 0.205), (0.018 * sx, 0.465, 0.19), "Mandible_%s_1" % s, True)
    for idx, hy, ky, ay, fy in LEGS:
        hip = (0.07 * sx, hy, 0.24)
        knee = (0.30 * sx, ky, 0.52)
        ankle = (0.47 * sx, ay, 0.20)
        foot = (0.53 * sx, fy, 0.0)
        bone("Leg_%s%s_1" % (s, idx), hip, knee, "Body")
        bone("Leg_%s%s_2" % (s, idx), knee, ankle, "Leg_%s%s_1" % (s, idx), True)
        bone("Leg_%s%s_3" % (s, idx), ankle, foot, "Leg_%s%s_2" % (s, idx), True)
bpy.ops.object.mode_set(mode="OBJECT")
mapping = {"Abdomen": "Abdomen", "Thorax": "Body", "Neck": "Body", "Head": "Head"}
for ob in bpy.data.objects:
    if ob.name.startswith(("Leg_", "Mandible_")):
        mapping[ob.name] = ob.name
for name, bname in mapping.items():
    ob = bpy.data.objects[name]
    mw = ob.matrix_world.copy()
    ob.parent = arm
    ob.parent_type = "BONE"
    ob.parent_bone = bname
    bpy.context.view_layer.update()
    ob.matrix_world = mw
bpy.context.view_layer.update()
print("rig: bones", len(arm_data.bones), "parts", len(mapping))
