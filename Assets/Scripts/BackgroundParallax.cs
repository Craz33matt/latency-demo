using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public string name = "Layer";
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public float speed = 0.5f;
        public bool moveOnTitleScreen = true;
        public float titleScreenSpeed = 0.5f;
    }

    public ParallaxLayer[] layers;
    public bool scaleWithGameSpeed = true;
    public float gameSpeedMultiplier = 1f;
    public float menuParallaxBaseSpeed = 1f;

    private void Reset()
    {
        var renderer = GetComponent<MeshRenderer>();
        var filter = GetComponent<MeshFilter>();
        if (renderer != null || filter != null)
        {
            layers = new ParallaxLayer[]
            {
                new ParallaxLayer
                {
                    name = "Background",
                    meshFilter = filter,
                    meshRenderer = renderer,
                    speed = 0.25f
                }
            };
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        bool isPlayingGame = GameManager.Instance != null && GameManager.Instance.enabled;
        float baseSpeed = isPlayingGame ? Mathf.Max(0f, GameManager.Instance.gameSpeed) : menuParallaxBaseSpeed;

        for (int i = 0; i < layers.Length; i++)
        {
            var layer = layers[i];
            if (layer == null)
                continue;

            float layerSpeed;
            if (isPlayingGame)
            {
                layerSpeed = baseSpeed;
            }
            else if (layer.moveOnTitleScreen)
            {
                layerSpeed = layer.titleScreenSpeed;
            }
            else
            {
                layerSpeed = 0f;
            }

            if (layer.meshFilter != null)
            {
                layerSpeed /= Mathf.Max(0.0001f, layer.meshFilter.transform.localScale.x);
            }
            layerSpeed *= layer.speed * gameSpeedMultiplier;

            var renderer = layer.meshRenderer;
            if (renderer == null && layer.meshFilter != null)
            {
                renderer = layer.meshFilter.GetComponent<MeshRenderer>();
            }

            if (renderer != null)
            {
                var mat = renderer.material;
                if (mat != null)
                {
                    var offset = mat.mainTextureOffset;
                    offset += Vector2.right * layerSpeed * Time.deltaTime;
                    mat.mainTextureOffset = offset;
                }
            }
        }
    }
}
