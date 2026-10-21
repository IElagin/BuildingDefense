# Step 5: clips on the "Bomber" armature, 24 fps. Every clip keys every bone (rest where unused),
# so no pose leaks between takes in the FBX and in Unity.
#   Walk  12 frames, loop  - tripod gait, body bob, abdomen sway, mandibles
#   Death 18 frames, once  - hop, flip onto the back (glowing belly up), legs curl (DeathProcessTime 0.8 s)
#   Idle  24 frames, loop  - the bomb abdomen pulses
#   Spawn 14 frames, once  - climbs out of the burrow with flailing legs
import bpy, math
from mathutils import Vector, Matrix, Quaternion

arm = bpy.data.objects["Bomber"]
if arm.animation_data is None:
    arm.animation_data_create()
TAU = 2 * math.pi
LEGS = [s + i for s in ("L", "R") for i in ("1", "2", "3")]
GROUP_A = {"L1", "R2", "L3"}


def local_q(pb, R):
    # a rotation given in armature axes, expressed in the bone's rest frame
    B = pb.bone.matrix_local.to_3x3()
    return (B.inverted() @ R @ B).to_quaternion()


def lift_axis(leg):
    b = arm.data.bones["Leg_%s_1" % leg]
    d = b.tail_local - b.head_local
    d.z = 0
    d.normalize()
    return d.cross(Vector((0, 0, 1))).normalized()


def smooth(x):
    x = max(0.0, min(1.0, x))
    return x * x * (3 - 2 * x)


def new_action(name):
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


def key_rest_for_unused(act):
    keyed = set()
    for fc in act.fcurves:
        if fc.data_path.startswith('pose.bones["'):
            keyed.add((fc.data_path.split('"')[1], fc.data_path.rsplit(".", 1)[1]))
    f0 = int(act.frame_range[0])
    for pb in arm.pose.bones:
        if (pb.name, "rotation_quaternion") not in keyed:
            pb.rotation_quaternion = Quaternion()
            pb.keyframe_insert("rotation_quaternion", frame=f0)
        if (pb.name, "location") not in keyed:
            pb.location = Vector()
            pb.keyframe_insert("location", frame=f0)
        if (pb.name, "scale") not in keyed:
            pb.scale = Vector((1, 1, 1))
            pb.keyframe_insert("scale", frame=f0)


body = arm.pose.bones["Body"]
ab = arm.pose.bones["Abdomen"]
root = arm.pose.bones["Root"]

# Walk
act = new_action("Walk")
N = 12
for f in range(N + 1):
    t = f / N
    for leg in LEGS:
        side = 1 if leg[0] == "R" else -1
        ph = (t + (0.0 if leg in GROUP_A else 0.5)) % 1.0
        swing = math.sin(TAU * ph) * math.radians(20) * side
        lift = max(0.0, math.cos(TAU * ph)) * math.radians(22)
        la = lift_axis(leg)
        pb1 = arm.pose.bones["Leg_%s_1" % leg]
        pb1.rotation_quaternion = local_q(pb1, Matrix.Rotation(swing, 3, "Z") @ Matrix.Rotation(lift, 3, la))
        pb1.keyframe_insert("rotation_quaternion", frame=f + 1)
        pb2 = arm.pose.bones["Leg_%s_2" % leg]
        pb2.rotation_quaternion = local_q(pb2, Matrix.Rotation(-lift * 0.6, 3, la))
        pb2.keyframe_insert("rotation_quaternion", frame=f + 1)
    body.location = Vector((0, 0, 0.012 * (1 - math.cos(2 * TAU * t)) / 2))
    body.rotation_quaternion = local_q(body, Matrix.Rotation(math.sin(TAU * t) * math.radians(3), 3, "Y"))
    body.keyframe_insert("location", frame=f + 1)
    body.keyframe_insert("rotation_quaternion", frame=f + 1)
    ab.rotation_quaternion = local_q(ab, Matrix.Rotation(math.sin(TAU * t - 0.6) * math.radians(5), 3, "Z") @ Matrix.Rotation(math.sin(2 * TAU * t) * math.radians(2), 3, "X"))
    ab.keyframe_insert("rotation_quaternion", frame=f + 1)
    for s in ("L", "R"):
        m = arm.pose.bones["Mandible_%s_1" % s]
        m.rotation_quaternion = local_q(m, Matrix.Rotation(math.sin(2 * TAU * t) * math.radians(10) * (1 if s == "L" else -1), 3, "Z"))
        m.keyframe_insert("rotation_quaternion", frame=f + 1)
key_rest_for_unused(act)

# Death
act = new_action("Death")
for f in (1, 3, 5, 7, 9, 11, 13, 15, 18):
    t = (f - 1) / 17.0
    hop = math.sin(min(1.0, t / 0.5) * math.pi) * 0.18
    flip = smooth(t / 0.55) * math.radians(165)
    body.location = Vector((0, 0, hop + smooth((t - 0.4) / 0.3) * 0.06))
    body.rotation_quaternion = local_q(body, Matrix.Rotation(flip, 3, "Y") @ Matrix.Rotation(smooth(t / 0.5) * math.radians(-12), 3, "X"))
    body.keyframe_insert("location", frame=f)
    body.keyframe_insert("rotation_quaternion", frame=f)
    ab.rotation_quaternion = local_q(ab, Matrix.Rotation(smooth(t / 0.6) * math.radians(14), 3, "X"))
    ab.scale = Vector((1, 1, 1)) * (1.0 - 0.06 * smooth(t))
    ab.keyframe_insert("rotation_quaternion", frame=f)
    ab.keyframe_insert("scale", frame=f)
    for leg in LEGS:
        la = lift_axis(leg)
        k = smooth(t / 0.7)
        twitch = math.sin(t * 9 * math.pi) * (1 - k) * math.radians(8)
        pb1 = arm.pose.bones["Leg_%s_1" % leg]
        pb2 = arm.pose.bones["Leg_%s_2" % leg]
        pb3 = arm.pose.bones["Leg_%s_3" % leg]
        pb1.rotation_quaternion = local_q(pb1, Matrix.Rotation(k * math.radians(25) + twitch, 3, la))
        pb2.rotation_quaternion = local_q(pb2, Matrix.Rotation(-k * math.radians(70), 3, la))
        pb3.rotation_quaternion = local_q(pb3, Matrix.Rotation(-k * math.radians(55), 3, la))
        for pb in (pb1, pb2, pb3):
            pb.keyframe_insert("rotation_quaternion", frame=f)
    for s in ("L", "R"):
        m = arm.pose.bones["Mandible_%s_1" % s]
        m.rotation_quaternion = local_q(m, Matrix.Rotation(smooth(t / 0.3) * math.radians(25) * (1 if s == "L" else -1), 3, "Z"))
        m.keyframe_insert("rotation_quaternion", frame=f)
key_rest_for_unused(act)

# Idle
act = new_action("Idle")
for f in range(0, 25, 3):
    t = f / 24.0
    ab.scale = Vector((1, 1, 1)) * (1.0 + 0.045 * (1 - math.cos(TAU * t)) / 2)
    ab.rotation_quaternion = local_q(ab, Matrix.Rotation(math.sin(TAU * t) * math.radians(2), 3, "X"))
    ab.keyframe_insert("scale", frame=f + 1)
    ab.keyframe_insert("rotation_quaternion", frame=f + 1)
    body.location = Vector((0, 0, 0.006 * math.sin(TAU * t)))
    body.keyframe_insert("location", frame=f + 1)
    for s in ("L", "R"):
        m = arm.pose.bones["Mandible_%s_1" % s]
        m.rotation_quaternion = local_q(m, Matrix.Rotation(max(0.0, math.sin(2 * TAU * t)) * math.radians(12) * (1 if s == "L" else -1), 3, "Z"))
        m.keyframe_insert("rotation_quaternion", frame=f + 1)
key_rest_for_unused(act)

# Spawn
act = new_action("Spawn")
for f in range(1, 15):
    t = (f - 1) / 13.0
    root.location = Vector((0, 0, -0.75 * (1 - smooth(t))))
    root.rotation_quaternion = local_q(root, Matrix.Rotation((1 - smooth(t)) * math.radians(-35), 3, "X"))
    root.keyframe_insert("location", frame=f)
    root.keyframe_insert("rotation_quaternion", frame=f)
    for leg in LEGS:
        la = lift_axis(leg)
        side = 1 if leg[0] == "R" else -1
        flail = math.sin(t * 6 * math.pi + (0 if leg in GROUP_A else math.pi)) * (1 - smooth(t)) * math.radians(30)
        pb1 = arm.pose.bones["Leg_%s_1" % leg]
        pb1.rotation_quaternion = local_q(pb1, Matrix.Rotation(flail * side, 3, "Z") @ Matrix.Rotation(abs(flail), 3, la))
        pb1.keyframe_insert("rotation_quaternion", frame=f)
key_rest_for_unused(act)

arm.animation_data.action = None
for pb in arm.pose.bones:
    pb.rotation_quaternion = Quaternion()
    pb.location = Vector()
    pb.scale = Vector((1, 1, 1))
print("clips", [(a.name, tuple(int(x) for x in a.frame_range)) for a in bpy.data.actions])
