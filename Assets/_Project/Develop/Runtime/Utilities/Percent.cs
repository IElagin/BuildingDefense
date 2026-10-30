namespace Assets._Project.Develop.Runtime.Utilities
{
    public static class Percent
    {
        private const float Whole = 100f;

        public static float ToShare(float percent) => percent / Whole;
    }
}
