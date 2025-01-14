using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for the HUD's information panel")]
    public class InfoPanelController : ImageController
    {
#pragma warning disable
        [SerializeField] private TextController score;
        [EmmyDoc("Gets the score text controller")]
        public TextController Score
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        [EmmyDoc("Gets the jacket image controller")]
        public ImageController Jacket
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        [EmmyDoc("Gets the title text controller")]
        public TitleController Title
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        [EmmyDoc("Gets the composer text controller")]
        public ComposerController Composer
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        [EmmyDoc("Gets the difficulty text controller")]
        public DifficultyController DifficultyText
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        [EmmyDoc("Gets the difficulty image controller")]
        public ImageController DifficultyBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyBackground);
                return difficultyBackground;
            }

#pragma warning restore
        }
    }
}