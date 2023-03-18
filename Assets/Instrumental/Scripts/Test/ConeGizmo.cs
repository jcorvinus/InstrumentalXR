using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeGizmo : MonoBehaviour
{
    [SerializeField] float angle = 20;
    [SerializeField] float distance=1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCone(Vector3 source, float length, float coneAngle, Vector3 normal)
    {
        Vector3 center = source + (normal * length);
        // so we want to draw a single circle at a specific distance.
        float radius = Mathf.Tan(coneAngle * Mathf.Deg2Rad) * length;

        DebugExtension.DrawCircle(center, normal, Gizmos.color, radius);

        // then draw our connecting lines
        float iter = 360 * 0.25f;
        for (int i = 0; i < 4; i++)
        {
            Vector3 startPoint = ((Vector3.forward) * radius);
            Vector3 destination = Quaternion.AngleAxis(i * iter, normal) *
                startPoint;
            Gizmos.DrawLine(source, source + (normal * length) + destination);
        }
    }

    private void OnDrawGizmos()
	{
        Gizmos.color = Color.blue;

        DrawCone(transform.position, distance, angle,
            transform.up);
	}
}
