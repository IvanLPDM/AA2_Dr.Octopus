using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inverse_Optimized : MonoBehaviour
{

    public List<Transform> Joints = new List<Transform>();
    public List<LineRenderer> LineRenderers = new List<LineRenderer>();
    private List<Vector3> Distances = new List<Vector3>();
    private float[] theta;

    public Transform target;
    public Transform spiderman;


    private float costFunction;

    public float alpha = 1f;

    public float tolerance = 1f;

    float[] gradient;

    // Start is called before the first frame update
    void Start()
    {
        int jointCount = Joints.Count;

        // Calcular distancias entre cada par de articulaciones
        for (int i = 0; i < jointCount - 1; i++)
        {
            Distances.Add(Joints[i + 1].position - Joints[i].position);
        }

        Transform endFactor = Joints.Last();

        // Inicializar ángulos
        theta = new float[jointCount - 1];

        costFunction = Vector3.Distance(endFactor.position, target.position) * Vector3.Distance(endFactor.position, target.position);

        // Inicializar renderizadores de línea
        foreach (Transform joint in Joints)
        {
            LineRenderer lineRenderer = joint.gameObject.AddComponent<LineRenderer>();
            InitializeLineRenderer(lineRenderer);
            LineRenderers.Add(lineRenderer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("cost function = " + costFunction);
        //Comprobar si hemos cogido el objeto
        if (costFunction > tolerance)
        {
            gradient = GetGradient(theta);

            for (int i = 0; i < theta.Length; i++)
            {
                theta[i] -= alpha * gradient[i];
            }


            Vector3[] newPosition = endFactorFunction(theta);

            // Actualiza las posiciones de las articulaciones
            for (int i = 1; i < Joints.Count; i++)
            {
                Joints[i].position = newPosition[i];
            }

        }
        //Cuando ya hemos cogido al objetivo (Animacion agarrar spiderman)
        else
        {
            capture();
        }

        //Actualizar la distacia de endFactor con el objeto
        costFunction = lossCostFunction(theta);

        UpdateVisualLinks();

    }

    void InitializeLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2; 
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void UpdateVisualLinks()
    {
        for (int i = 0; i < Joints.Count - 1; i++)
        {
            LineRenderers[i].SetPosition(0, Joints[i].position);
            LineRenderers[i].SetPosition(1, Joints[i + 1].position);
        }
    }

    Vector3[] endFactorFunction(float[] theta)
    {
        Vector3[] positions = new Vector3[Joints.Count];
        positions[0] = Joints[0].position; // La primera posición es fija

        // Rotaciones acumulativas
        Quaternion rotations = Quaternion.identity;

        // Calcular posiciones de las articulaciones
        for (int i = 1; i < Joints.Count; i++)
        {
            // Aplica rotación sobre Y (usando el índice i-1 de theta)
            rotations *= Quaternion.AngleAxis(theta[i - 1], Vector3.up);    // Rotación sobre Y

            // Aplica rotación sobre X (si deseas, usa otro ángulo de theta)
            if (i < theta.Length) // Asegúrate de que no se salga del rango
                rotations *= Quaternion.AngleAxis(theta[i], Vector3.right);     // Rotación sobre X

            // Aplica rotación sobre Z solo si el índice es válido
            if (i + 1 < theta.Length) // Asegúrate de que no se salga del rango
                rotations *= Quaternion.AngleAxis(theta[i + 1], Vector3.forward); // Rotación sobre Z

            // Calcula la nueva posición con la rotación acumulada
            positions[i] = positions[i - 1] + rotations * Distances[i - 1]; // Nueva posición
        }

        return positions;
    }

    float lossCostFunction(float[] theta)
    {
        //Calculamos la posición de endFactor
        Vector3 endpostion = endFactorFunction(theta)[Joints.Count - 1];

        return Vector3.Distance(endpostion, target.position) * Vector3.Distance(endpostion, target.position);
    }


    float[] GetGradient(float[] theta)
    {

        float[] gradient = new float[theta.Length];
        float step = 1e-2f;

        for (int i = 0; i < theta.Length; i++)
        {
            float originalTheta = theta[i];

            // Calcula la derivada parcial
            theta[i] += step;
            float costPlus = lossCostFunction(theta);

            theta[i] = originalTheta; // Restaura el valor original
            float cost = lossCostFunction(theta);

            gradient[i] = (costPlus - cost) / step;
        }

        // Normaliza el gradiente
        float magnitude = Mathf.Sqrt(gradient.Sum(g => g * g));
        for (int i = 0; i < gradient.Length; i++)
        {
            gradient[i] /= magnitude;
        }

        return gradient;
    }



    void capture()
    {
        spiderman.GetComponent<Animator>().enabled = false;
    }
}
