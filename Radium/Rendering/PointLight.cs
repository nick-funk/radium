namespace Radium.Rendering
{
    public class PointLight
    {
        public PointLight(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; set; }
    }
}