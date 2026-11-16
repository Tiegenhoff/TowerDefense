using UnityEngine;

// Put this on an enemy and assign its material sprite in the Inspector.
public class EnemyMaterialDrop : MonoBehaviour
{
    public Sprite materialSprite;
    public Color spriteColor = Color.white;
    public Vector3 spriteScale = Vector3.one;
    public float materialZ = -0.9f;
    [Tooltip("Movement speed in screen heights per second.")]
    public float flySpeed = 0.8f;
    [Tooltip("The material is drawn behind this enemy's sprite.")]
    public SpriteRenderer enemySprite;
    private bool dropped;

    // Called when an enemy is defeated, before it is destroyed.
    public void DropMaterial()
    {
        if (dropped || !isActiveAndEnabled) return;
        dropped = true;
        if (materialSprite == null)
        {
            Debug.LogWarning("Assign a material sprite to " + name + ".", this);
            return;
        }
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("A camera tagged MainCamera is needed for the material drop.", this);
            return;
        }

        GameObject material = new GameObject("Flying Material");
        material.transform.position = new Vector3(transform.position.x, transform.position.y, materialZ);
        material.transform.localScale = spriteScale;
        SpriteRenderer sprite = material.AddComponent<SpriteRenderer>();
        sprite.sprite = materialSprite;
        sprite.color = spriteColor;
        SpriteRenderer reference = enemySprite != null ? enemySprite : GetComponentInChildren<SpriteRenderer>();
        if (reference != null)
        {
            sprite.sortingLayerID = reference.sortingLayerID;
            sprite.sortingOrder = reference.sortingOrder - 1;
        }
        FlyingMaterial flying = material.AddComponent<FlyingMaterial>();
        flying.Setup(camera, flySpeed, materialZ);
    }
}

// The effect has its own GameObject, so it survives the enemy being destroyed.
public class FlyingMaterial : MonoBehaviour
{
    private Camera targetCamera;
    private float speed;
    private float zPosition;
    private Vector2 screenPosition;

    public void Setup(Camera camera, float flySpeed, float materialZ)
    {
        targetCamera = camera;
        speed = Mathf.Max(0.01f, flySpeed);
        zPosition = materialZ;
        Vector3 screen = camera.WorldToScreenPoint(transform.position);
        screenPosition = new Vector2(screen.x, screen.y);
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            Destroy(gameObject);
            return;
        }
        Rect area = targetCamera.pixelRect;
        Vector2 destination = new Vector2(area.xMax + area.width * 0.1f, area.yMax + area.height * 0.1f);
        screenPosition = Vector2.MoveTowards(screenPosition, destination, speed * area.height * Time.deltaTime);

        float depth = targetCamera.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, zPosition)).z;
        Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
        world.z = zPosition;
        transform.position = world;

        if (screenPosition.x > area.xMax && screenPosition.y > area.yMax)
            Destroy(gameObject);
    }
}
