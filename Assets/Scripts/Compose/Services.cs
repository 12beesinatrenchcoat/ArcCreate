using ArcCreate.Compose.Components;
using ArcCreate.Compose.Project;
using UnityEngine;

namespace ArcCreate.Compose
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private ProjectService project;
        [SerializeField] private ColorPickerWindow colorPicker;

        public static IProjectService Project { get; private set; }

        public static IColorPickerService ColorPicker { get; private set; }

        private void Awake()
        {
            Project = project;
            ColorPicker = colorPicker;
        }
    }
}