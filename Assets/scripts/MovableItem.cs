using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MovableItem : MonoBehaviour
{
    public string FruitName;
    public float backduration = 3f;
    public PlacementPlatform _sp;
    public float height = 1.5f;
    public Animator myAnimator;
    private Material fruitMaterial;
    private Color originalColor;

    [Header("Match Animation Settings")]
    public float fadeOutDuration = 0.5f;
    public float destroyDelay = 1.2f;
    public float rotationSpeed = 360f;
    public float bounceHeight = 0.5f;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("Drag & Throw Settings")]
    public float maxDragHeight = 0.3f;
    public float flingMaxSpeed = 0.5f;
    public float backForceMultiplier = 0.3f;

    [Header("Game Area Bounds")]
    public Vector3 minBoundary = new Vector3(-8.26f, -3.80f, -7.20f);
    public Vector3 maxBoundary = new Vector3(6.56f, -3.00f, 3.40f);

    private Vector3 startposition;
    private Vector3 fallposition;
    private Camera mainCamera;
    private Rigidbody rb;

    private bool isDragging = false;
    private float elapsedTime = 0;
    private bool isBack = false;
    private Vector3 screenPoint;
    private Vector3 offset;
    private float initialY;
    private Vector3 velocityBeforeKinematic = Vector3.zero;

    private void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        // Get the renderer's material
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            fruitMaterial = renderer.material;
            if (fruitMaterial.HasProperty("_Color"))
            {
                originalColor = fruitMaterial.color;
            }
            else
            {
                originalColor = Color.white;
            }
        }

        fallposition = transform.position;
        initialY = transform.position.y;
        startposition = transform.position;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        rb.isKinematic = true;

        screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 rawOffset = transform.position
        - mainCamera.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            screenPoint.z));

        offset = new Vector3(rawOffset.x, 0f, rawOffset.z);
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 currentScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 currentPosition = mainCamera.ScreenToWorldPoint(currentScreenPoint) + offset;

            currentPosition.y = Mathf.Clamp(currentPosition.y, initialY, initialY + maxDragHeight);
            currentPosition.x = Mathf.Clamp(currentPosition.x, minBoundary.x, maxBoundary.x);
            currentPosition.y = Mathf.Clamp(currentPosition.y, minBoundary.y, maxBoundary.y);
            currentPosition.z = Mathf.Clamp(currentPosition.z, minBoundary.z, maxBoundary.z);

            rb.MovePosition(currentPosition);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        rb.isKinematic = false;

        Vector3 newVelocity = rb.linearVelocity;
        if (newVelocity.magnitude > flingMaxSpeed)
        {
            newVelocity = newVelocity.normalized * flingMaxSpeed;
        }
        rb.linearVelocity = newVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Placement Area")
        {
            _sp = other.GetComponent<PlacementPlatform>();

            if (_sp.CurrentFruit == null)
            {
                _sp.CurrentFruit = this;
            }
            else if (_sp.CurrentFruit != this && _sp.CurrentFruit.FruitName == this.FruitName)
            {
                StartCoroutine(HandleMatch());
            }
            else if (_sp.CurrentFruit != this && _sp.CurrentFruit.FruitName != this.FruitName)
            {
                isBack = true;
                velocityBeforeKinematic = rb.linearVelocity * backForceMultiplier;
                rb.isKinematic = true;
            }
        }

        if (other.transform.name == "Destory Trigger Area")
        {
            Destroy(gameObject);
            if (_sp.Fruits.childCount <= 1)
            {
                _sp.ComplatePanel.SetActive(true);
            }
        }

        if (other.transform.name == "WrongFallArea")
        {
            transform.position = fallposition;
        }
    }

    private IEnumerator HandleMatch()
    {
        // Store reference to other fruit before nulling it
        MovableItem otherFruit = _sp.CurrentFruit;
        
        // Make fruits kinematic for controlled animation
        rb.isKinematic = true;
        otherFruit.rb.isKinematic = true;
        
        // Update game state
        gameObject.layer = 6;
        otherFruit.gameObject.layer = 6;
        _sp.CurrentFruit = null;
        _sp.AddScore(10);

        // Start the animation coroutine for both fruits
        StartCoroutine(PlayMatchAnimation(transform));
        StartCoroutine(PlayMatchAnimation(otherFruit.transform));

        // Enhanced particle effects at both positions
        if (_sp.matchParticleEffect)
        {
            SpawnParticleEffects(transform.position);
            SpawnParticleEffects(otherFruit.transform.position);
        }

        // Wait for animation to complete before destroying
        yield return new WaitForSeconds(destroyDelay);

        // Check for game completion
        if (_sp.Fruits.childCount <= 2)
        {
            _sp.ComplatePanel.SetActive(true);
        }
        
        // Destroy both fruits
        Destroy(otherFruit.gameObject);
        Destroy(gameObject);
    }

    private void SpawnParticleEffects(Vector3 position)
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = i * (360f / 4);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 0.5f;
            Instantiate(_sp.matchParticleEffect, position + offset, Quaternion.Euler(0, angle, 0));
        }
    }

    public IEnumerator PlayMatchAnimation(Transform fruitTransform)
    {
        Vector3 startPos = fruitTransform.position;
        Vector3 startScale = fruitTransform.localScale;
        Renderer fruitRenderer = fruitTransform.GetComponent<Renderer>();
        Material fruitMat = fruitRenderer ? fruitRenderer.material : null;
        Color originalFruitColor = fruitMat ? fruitMat.color : Color.white;

        // Initial flash
        if (fruitMat != null)
        {
            fruitMat.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            fruitMat.color = originalFruitColor;
        }

        // Trigger base animation
        Animator fruitAnimator = fruitTransform.GetComponent<Animator>();
        if (fruitAnimator != null)
            fruitAnimator.SetTrigger("OnMatch");

        // Main animation loop
        float elapsedTime = 0;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            
            // Smooth step for better easing
            float smoothT = t * t * (3f - 2f * t);
            
            // Scale with bounce
            float scale = 1 + Mathf.Sin(t * Mathf.PI * 2) * 0.5f;
            fruitTransform.localScale = startScale * scale;
            
            // Rotation
            fruitTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // Bounce movement
            float bounce = Mathf.Sin(t * Mathf.PI * 2) * bounceHeight;
            fruitTransform.position = startPos + Vector3.up * bounce;
            
            // Fade out
            if (fruitMat != null)
            {
                Color endColor = new Color(originalFruitColor.r, originalFruitColor.g, originalFruitColor.b, 0);
                fruitMat.color = Color.Lerp(originalFruitColor, endColor, smoothT);
            }

            yield return null;
        }

        // Final flash
        if (fruitMat != null)
        {
            fruitMat.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            fruitMat.color = new Color(originalFruitColor.r, originalFruitColor.g, originalFruitColor.b, 0);
        }
    }

    private void Update()
    {
        if (isBack)
        {
            if (elapsedTime < backduration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / backduration;

                Vector3 horizontalPosition = Vector3.Lerp(transform.position, startposition, t);
                float arc = Mathf.Sin(t * Mathf.PI) * height;

                transform.position = new Vector3(
                    horizontalPosition.x,
                    horizontalPosition.y + arc,
                    horizontalPosition.z
                );
            }
            else
            {
                rb.isKinematic = false;
                rb.linearVelocity = velocityBeforeKinematic;

                isBack = false;
                elapsedTime = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_sp != null && _sp.CurrentFruit == this)
        {
            _sp.CurrentFruit = null;
        }
    }
}
