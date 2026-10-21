# World step 6: rigid-part rigs and looping clips for the animated world assets.
#   Dome (armature "Dome"): Root, Dish (turns clockwise once per loop), Tip (pulses twice); clip Idle, 48 frames.
#   Mine (armature "Mine", placed at the mine's authoring slot): Root, Light (pulses); clip Armed, 24 frames.
# Parts follow their bones by bone parenting, so Unity imports plain MeshRenderers. Every bone is keyed.
import bpy, math
from mathutils import Vector, Matrix, Quaternion

scene = bpy.context.scene
TAU = 2 * math.pi


def build_rig(name, location, bones, parts):
    arm_data = bpy.data.armatures.new(name + "Rig")
    arm = bpy.data.objects.new(name, arm_data)
    scene.collection.objects.link(arm)
    arm.location = location
    arm.show_in_front = True
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    bpy.context.view_layer.objects.active = arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for bone_name, head, tail, parent in bones:
        b = arm_data.edit_bones.new(bone_name)
        b.head = Vector(head)
        b.tail = Vector(tail)
        if parent:
            b.parent = arm_data.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT")
    bpy.context.view_layer.update()
    for part, bone_name in parts.items():
        ob = bpy.data.objects[part]
        mw = ob.matrix_world.copy()
        ob.parent = arm
        ob.parent_type = "BONE"
        ob.parent_bone = bone_name
        bpy.context.view_layer.update()
        ob.matrix_world = mw
    arm.animation_data_create()
    return arm


def local_q(pb, R):
    B = pb.bone.matrix_local.to_3x3()
    return (B.inverted() @ R @ B).to_quaternion()


def new_action(arm, name):
    old = bpy.data.actions.get(name)
    if old:
        bpy.data.actions.remove(old)
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    arm.animation_data.action = act
    for pb in arm.pose.bones:
        pb.rotation_mode = "QUATERNION"
        pb.rotation_quaternion = Quaternion()
        pb.location = Vector()
        pb.scale = Vector((1, 1, 1))
    return act


def key_all(arm, frame):
    for pb in arm.pose.bones:
        pb.keyframe_insert("rotation_quaternion", frame=frame)
        pb.keyframe_insert("location", frame=frame)
        pb.keyframe_insert("scale", frame=frame)


def finish(arm):
    arm.animation_data.action = None
    for pb in arm.pose.bones:
        pb.rotation_quaternion = Quaternion()
        pb.location = Vector()
        pb.scale = Vector((1, 1, 1))


dome = build_rig(
    "Dome", (0, 0, 0),
    (("Root", (0, 0, 0), (0, 0.5, 0), None),
     ("Dish", (0, 0, 2.15), (0, 0, 2.4), "Root"),
     ("Tip", (0, 0, 2.5), (0, 0, 2.7), "Root")),
    {"Dome_Plinth": "Root", "Dome_Shell": "Root", "Dome_Door": "Root", "Dome_Tanks": "Root",
     "Dome_Mast": "Root", "Dome_Dish": "Dish", "Dome_Tip": "Tip"})
new_action(dome, "Idle")
for f in range(0, 49):  # every frame keyed: an even turn
    t = f / 48.0
    dish = dome.pose.bones["Dish"]
    # clockwise seen from above (user, 2026-10-21): negative turn about +Z
    dish.rotation_quaternion = local_q(dish, Matrix.Rotation(-t * TAU, 3, "Z"))
    tip = dome.pose.bones["Tip"]
    tip.scale = Vector((1, 1, 1)) * (1.0 + 0.55 * max(0.0, math.sin(2 * TAU * t)) ** 2)
    key_all(dome, f + 1)
finish(dome)

mine = build_rig(
    "Mine", (4.5, 0, 0),
    (("Root", (0, 0, 0), (0, 0.3, 0), None),
     ("Light", (0, 0, 0.19), (0, 0, 0.3), "Root")),
    {"Mine_Body": "Root", "Mine_TriggerRing": "Root", "Mine_Light": "Light"})
new_action(mine, "Armed")
for f in range(0, 25, 2):
    t = f / 24.0
    light = mine.pose.bones["Light"]
    light.scale = Vector((1, 1, 1)) * (1.0 + 0.6 * max(0.0, math.sin(TAU * t)) ** 3)
    key_all(mine, f + 1)
finish(mine)
print("rigs", [(a.name, tuple(int(x) for x in a.frame_range)) for a in bpy.data.actions])
