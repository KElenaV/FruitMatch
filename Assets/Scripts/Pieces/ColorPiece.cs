using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchForThree
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ColorPiece : MonoBehaviour
    {
        [System.Serializable]
        public struct ColorSprite
        {
            public ColorType color;
            public Sprite sprite;
        }

        [SerializeField] private ColorSprite[] _colorSprites;
        [SerializeField] private SpriteRenderer _sprite;
       
        private ColorType _color;
        private Dictionary<ColorType, Sprite> _colorSpriteDict;

        public ColorType Color
        {
            get => _color;
            set => SetColor(value);
        }

        public int NumColors => _colorSprites.Length;

        private void Awake ()
        {
            _colorSpriteDict = _colorSprites
                .GroupBy(s => s.color)
                .ToDictionary(group => group.Key, group => group.First().sprite);
        }

        public void SetColor(ColorType color)
        {
            _color = color;
            _sprite.sprite = (_sprite != null && _colorSpriteDict.ContainsKey(color)) ? _colorSpriteDict[color] : null;
        }
    }
}
