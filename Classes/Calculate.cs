using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using ValveResourceFormat.ResourceTypes.ModelAnimation;
using static Titled_Gui.Modules.Visual.BoneESP;
using Bone = Titled_Gui.Data.Entity.Types.Bone;

namespace Titled_Gui.Classes
{
    public static class Calculate
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos)// this seemed slightly better or same idk
        {
            // calculate depth
            float view = matrix[3 * 4 + 0] * pos.X + matrix[3 * 4 + 1] * pos.Y + matrix[3 * 4 + 2] * pos.Z + matrix[3 * 4 + 3];

            // if entity is not visible
            if (view <= 0.01f)
            {
                // if entity is not visible
                return new(-99, -99);
            }

            // calculate screen x and y
            float screenX = matrix[0 * 4 + 0] * pos.X + matrix[0 * 4 + 1] * pos.Y + matrix[0 * 4 + 2] * pos.Z + matrix[0 * 4 + 3];
            float screenY = matrix[1 * 4 + 0] * pos.X + matrix[1 * 4 + 1] * pos.Y + matrix[1 * 4 + 2] * pos.Z + matrix[1 * 4 + 3];

            // perspective division 
            float halfW = GameState.renderer.ScreenSize.X * 0.5f;
            float halfH = GameState.renderer.ScreenSize.Y * 0.5f;

            float X = halfW + (screenX / view) * halfW;
            float Y = halfH - (screenY / view) * halfH;

            return new(X, Y);
        }

        public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
        {
            Vector3 delta = new(to.X - from.X, to.Y - from.Y, to.Z - from.Z);

            float Distance = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            float pitch = (float)(-Math.Atan2(delta.Z, Distance) * 180 / Math.PI);
            float yaw = (float)(Math.Atan2(delta.Y, delta.X) * 180 / Math.PI);

            pitch = NormalizeAngle(pitch);
            yaw = NormalizeAngle(yaw);

            return new(pitch, yaw);
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
        private static int[] BonesToCheck = {0,5,6,8,9,11,13,14,16,23,26,27};
      
        public static List<Bone> ReadBones(nint boneAddress, float[] viewMatrix)
        {
            byte[] boneBytes = GameState.swed.ReadBytes(boneAddress, 27 * 32 + 16);
            List<Bone> bones = new();

            foreach (var boneId in Enum.GetValues<BoneIds>())
            {
                float x = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 0);
                float y = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 4);
                float z = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 8);
                Vector3 currentBone = new(x, y, z);
                Bone bone = new()
                {
                    Position = currentBone,
                };
                //if (BonesToCheck.Contains((int)boneId))
                //    bone.IsVisible = VisibilityCheck.Visible(GameState.LocalPlayer.EyePosition, bone.Position);

                bone.Position2D = WorldToScreen(viewMatrix, bone.Position);
                bones.Add(bone);
            }
            return bones;
        }

        public static List<Vector2> ReadBones2D(List<Bone> bones)
        {
            List<Vector2> bones2d = [];
            foreach (Bone bone in bones)
            {
                bones2d.Add(bone.Position2D);
            }
            return bones2d;
        }
    }
}
