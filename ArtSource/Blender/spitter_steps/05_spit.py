# Spitter step 5b: the Spit clip on the "Bomber" armature, after bomber_steps/05_clips.py. 12 frames at 24 fps
# match AttackProcessTime 0.5 s; the lunge peaks at frame 8 (~0.3 s, AttackDelayTime), when the blob leaves the spout.
#   frames 1-6  rear up, the acid sac swells
#   frames 6-8  lunge forward and down, the sac squeezes
#   frames 8-12 settle back to rest
import bpy, math
from mathutils import Vector, Matrix, Quaternion

arm = bpy.data.objects["Bomber"]


def local_q(pb, R):
    B = pb.bone.matrix_local.to_3x3()
    return (B.inverted() @ R @ B).to_quaternion()


def smooth(x):
    x = max(0.0, min(1.0, x))
    return x * x * (3 - 2 * x)


old = bpy.data.actions.get("Spit")
if old:
    bpy.data.actions.remove(old)
act = bpy.data.actions.new("Spit")
act.use_fake_user = True
arm.animation_data.action = act
for pb in arm.pose.bones:
    pb.rotation_mode = "QUATERNION"
    pb.rotation_quaternion = Quaternion()
    pb.location = Vector()
    pb.scale = Vector((1, 1, 1))

body = arm.pose.bones["Body"]
ab = arm.pose.bones["Abdomen"]
head = arm.pose.bones["Head"]
for f in range(1, 13):
    if f <= 6:
        k = smooth((f - 1) / 5.0)          # rear up
    elif f <= 8:
        k = 1.0 - 2.2 * smooth((f - 6) / 2.0)  # whip down past rest
    else:
        k = -1.2 * (1.0 - smooth((f - 8) / 4.0))
    body.rotation_quaternion = local_q(body, Matrix.Rotation(k * math.radians(-14), 3, "X"))
    body.location = Vector((0, -0.03 * max(0.0, k) + 0.04 * max(0.0, -k), 0.03 * max(0.0, k)))
    head.rotation_quaternion = local_q(head, Matrix.Rotation(k * math.radians(-10), 3, "X"))
    swell = 1.0 + 0.10 * max(0.0, k) - 0.08 * max(0.0, -k)
    ab.scale = Vector((swell, swell, swell))
    ab.rotation_quaternion = local_q(ab, Matrix.Rotation(k * math.radians(8), 3, "X"))
    for pb, path in ((body, "rotation_quaternion"), (body, "location"), (head, "rotation_quaternion"),
                     (ab, "scale"), (ab, "rotation_quaternion")):
        pb.keyframe_insert(path, frame=f)
    for s in ("L", "R"):
        m = arm.pose.bones["Mandible_%s_1" % s]
        m.rotation_quaternion = local_q(m, Matrix.Rotation(max(0.0, -k) * math.radians(28) * (1 if s == "L" else -1), 3, "Z"))
        m.keyframe_insert("rotation_quaternion", frame=f)
keyed = {(fc.data_path.split('"')[1], fc.data_path.rsplit(".", 1)[1]) for fc in act.fcurves}
for pb in arm.pose.bones:
    if (pb.name, "rotation_quaternion") not in keyed:
        pb.keyframe_insert("rotation_quaternion", frame=1)
    if (pb.name, "location") not in keyed:
        pb.keyframe_insert("location", frame=1)
    if (pb.name, "scale") not in keyed:
        pb.keyframe_insert("scale", frame=1)

arm.animation_data.action = None
for pb in arm.pose.bones:
    pb.rotation_quaternion = Quaternion()
    pb.location = Vector()
    pb.scale = Vector((1, 1, 1))
print("clips", [(a.name, tuple(int(x) for x in a.frame_range)) for a in bpy.data.actions])
