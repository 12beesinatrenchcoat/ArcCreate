using UnityEngine;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for handling resizing the viewport.
    /// </summary>
    public class GameplayViewport : MonoBehaviour
    {
        [SerializeField] private Camera editorCamera;
        [SerializeField] private RectTransform viewport;
        private readonly Vector3[] corners = new Vector3[4];

        private void Update()
        {
            if (Services.Gameplay == null)
            {
                return;
            }

            viewport.GetWorldCorners(corners);

            Vector3 bottomLeft = editorCamera.WorldToScreenPoint(corners[0]);
            Vector3 topRight = editorCamera.WorldToScreenPoint(corners[2]);

            float x = bottomLeft.x;
            float y = bottomLeft.y;
            float width = topRight.x - bottomLeft.x;
            float height = topRight.y - bottomLeft.y;

            Rect normalized = new Rect(
                x / Screen.width,
                y / Screen.height,
                width / Screen.width,
                height / Screen.height);

            Services.Gameplay.SetCameraViewportRect(normalized);
        }
    }
}