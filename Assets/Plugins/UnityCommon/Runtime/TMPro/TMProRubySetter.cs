using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// TMProにルビ付きのテキストを設定する
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMProRubySetter : MonoBehaviour
    {
        private const int RUBY_SIZE = 50;

        [SerializeField, TextArea(5, 10)] private string _UneditedText;
        [SerializeField] private bool _UseEasyBestFit = false;
        [SerializeField] private float _FontSizeMin = 18;
        [SerializeField] private float _FontSizeMax = 72;

        private bool _IsDirty = false;
        private readonly List<TextNode> _TextNodes = new();

        private TMP_Text _Target;
        private TMP_Text Target
        {
            get {
                if (_Target == null) { _Target = GetComponent<TMP_Text>(); }
                return _Target;
            }
        }

        /// <summary>
        /// ruby要素を含むテキスト
        /// </summary>
        public string UneditedText
        {
            get => _UneditedText;
            set {
                _UneditedText = value;
                _IsDirty = true;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _IsDirty = true;
        }
#endif

        private void Update()
        {
            if (_IsDirty || Target.havePropertiesChanged) {
                _IsDirty = false;

                // テキストをruby要素とそれ以外で分割する
                _TextNodes.Clear();
                if (!string.IsNullOrEmpty(_UneditedText)) {
                    var regex = new Regex(@"<ruby=""?(?<ruby>.*?)""?>(?<word>.*?)<\/ruby>");
                    MatchCollection matches = regex.Matches(_UneditedText);
                    int unmatchTextIndex = 0;
                    foreach (Match match in matches) {
                        if (unmatchTextIndex < match.Index) {
                            _TextNodes.Add(new TextNode(_UneditedText[unmatchTextIndex..match.Index]));
                        }
                        _TextNodes.Add(new RubyNode(match.Groups["word"].Value, match.Groups["ruby"].Value));
                        unmatchTextIndex = match.Index + match.Length;
                    }
                    if (unmatchTextIndex < _UneditedText.Length) {
                        _TextNodes.Add(new TextNode(_UneditedText[unmatchTextIndex..]));
                    }
                }

                // ruby要素をTMProのリッチテキストタグに変換したテキストを設定する
                Target.text = string.Concat(_TextNodes.Select(text => text.GetConvertedText(Target)));

                if (Target.enableAutoSizing) {
                    // AutoSize有効時はForceMeshUpdate()内でフォントサイズが調整されるため
                    // ForceMeshUpdate()を実行したあとにリッチテキストタグを設定しなおす
                    Target.ForceMeshUpdate();
                    Target.text = string.Concat(_TextNodes.Select(text => text.GetConvertedText(Target)));
                } else if (_UseEasyBestFit) {
                    Target.EasyBestFit(_FontSizeMin, _FontSizeMax);
                }
            }

            // ルビの表示が中途半端にならないようにmaxVisibleCharactersを調整する
            int characterIndex = 0;
            foreach (TextNode textNode in _TextNodes) {
                characterIndex += textNode.CharacterCount;
                if (textNode is RubyNode rubyNode) {
                    int nextCharacterIndex = characterIndex + rubyNode.Ruby.Length;
                    if (characterIndex <= Target.maxVisibleCharacters && Target.maxVisibleCharacters < nextCharacterIndex) {
                        Target.maxVisibleCharacters = nextCharacterIndex;
                        break;
                    }
                    characterIndex = nextCharacterIndex;
                }
            }

            if (Target.havePropertiesChanged) {
                Target.ForceMeshUpdate();
            }
        }

        private void OnDestroy()
        {
            _Target = null;
        }

        private class TextNode
        {
            public string Text { get; private set; }
            public int CharacterCount { get; private set; }

            public TextNode(string text)
            {
                Text = text;
                CharacterCount = text.RemoveRichTextTags().Length;
            }

            public virtual string GetConvertedText(TMP_Text target)
            {
                return Text;
            }
        }

        private class RubyNode : TextNode
        {
            public string Ruby { get; private set; }

            public RubyNode(string text, string ruby) : base(text)
            {
                Ruby = ruby;
            }

            public override string GetConvertedText(TMP_Text target)
            {
                float scale = (target.isOrthographic ? 1 : 10)
                    * (target.enableAutoSizing ? target.fontSize / target.fontSizeMax : 1);
                float textWidth = target.GetPreferredValues(Text).x * scale;
                float rubyWidth = target.GetPreferredValues(Ruby).x * scale * RUBY_SIZE / 100;
                float rubyOffset = -(textWidth + rubyWidth) * 0.5f;
                string rubiedText = textWidth < rubyWidth
                    ? $"<nobr><space={(rubyWidth - textWidth) * 0.5f}>{Text}<space={rubyOffset}><voffset=1em><size={RUBY_SIZE}%>{Ruby}</size></voffset></nobr>"
                    : $"<nobr>{Text}<space={rubyOffset}><voffset=1em><size={RUBY_SIZE}%>{Ruby}</size></voffset><space={(textWidth - rubyWidth) * 0.5f}></nobr>";
                return rubiedText;
            }
        }
    }
}
