using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnime : MonoBehaviour
{
    [SerializeField, Header("アニメーションリスト")]
    private Sprite[] sprites = new Sprite[0];

    [SerializeField, Header("アニメーション速度"),Range(1,100)]
    private float speed = 50f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sprites.Length < 1) return;

        var index = (int)Mathf.Repeat(Time.frameCount * speed / 100, sprites.Length);
        var renderer = GetComponent<MeshRenderer>();
        var material = renderer.material;

        var sprite = sprites[index];
        var texSize = new Vector2(sprite.texture.width, sprite.texture.height);
        var rect = sprite.textureRect;
        material.mainTexture = sprite.texture;
        material.mainTextureOffset = new Vector2(rect.x/ texSize.x, rect.y / texSize.y);
        material.mainTextureScale = new Vector2(rect.width / texSize.x, rect.height / texSize.y);
        //material.SetTextureOffset("_MainTex", new Vector2(rect.x, rect.y));
        //material.SetTextureScale("_MainTex", new Vector2(1 / rect.width, 1 / rect.height));

        renderer.material = material;
    }
}
