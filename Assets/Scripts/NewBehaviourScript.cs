using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform endFactor;


    public Transform target;

    public LineRenderer lineRenderer1;  
    public LineRenderer lineRenderer2;  
    public LineRenderer lineRenderer3;  


    private float costFunction;

    private Vector3 D1;
    private Vector3 D2;
    private Vector3 D3;

    public float alpha = 1f;

    private Vector4 theta;

    public float tolerance = 1f;

    private Vector4 gradient;
    // Start is called before the first frame update
    void Start()
    {

        D1 = Joint1.position - Joint0.position;
        D2 = Joint2.position - Joint1.position;
        D3 = endFactor.position - Joint2.position;

        theta = Vector4.zero;

        costFunction = Vector3.Distance(endFactor.position, target.position) * Vector3.Distance(endFactor.position, target.position);
        
        InitializeLineRenderer(lineRenderer1);
        InitializeLineRenderer(lineRenderer2);
        InitializeLineRenderer(lineRenderer3);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("cost function = " + costFunction);
        //Comprobar si hemos cogido el objeto
        if (costFunction > tolerance)
        {

            gradient = GetGradient(theta);
            theta -= alpha * gradient;

            Vector3[] newPosition = endFactorFunction(theta);
            Joint1.position = newPosition[0];
            Joint2.position = newPosition[1];
            endFactor.position = newPosition[2];

        }
        //Cuando ya hemos cogido al objetivo (Animacion agarrar spiderman)
        else
        {

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
        lineRenderer1.SetPosition(0, Joint0.position); 
        lineRenderer1.SetPosition(1, Joint1.position); 

        lineRenderer2.SetPosition(0, Joint1.position); 
        lineRenderer2.SetPosition(1, Joint2.position);

        lineRenderer3.SetPosition(0, Joint2.position);
        lineRenderer3.SetPosition(1, endFactor.position);

    }

    Vector3[] endFactorFunction(Vector4 theta)
    {
        Quaternion[] q = new Quaternion[4];
        q[0] = Quaternion.AngleAxis(theta.x, Vector3.up);
        q[1] = Quaternion.AngleAxis(theta.y, Vector3.forward);
        q[2] = Quaternion.AngleAxis(theta.z, Vector3.up);
        q[3] = Quaternion.AngleAxis(theta.w, Vector3.forward);


        Vector3 j1 = Joint0.position + q[0] * q[1] * D1;
        Vector3 j2 = j1 + q[0] * q[1] * q[2] * D2;
        Vector3 endfactor = j2 + q[0] * q[1] * q[2] * q[3] * D3;

        Vector3[] result = new Vector3[3];

        result[0] = j1;
        result[1] = j2;
        result[2] = endfactor;

        return result;
    }

    float lossCostFunction(Vector4 theta)
    {
        //Calculamos la posición de endFactor
        Vector3 endpostion = endFactorFunction(theta)[2];

        return Vector3.Distance(endpostion, target.position) * Vector3.Distance(endpostion, target.position);
    }


    Vector4 GetGradient(Vector4 theta)
    {

        Vector4 gradientVector;
        float step = 1e-2f;

        //x
        Vector4 thetaPlus = theta;
        thetaPlus.x = theta.x + step;
        gradientVector.x = (lossCostFunction(thetaPlus) - lossCostFunction(theta)) / step;

        // y
        thetaPlus = theta;
        thetaPlus.y = theta.y + step;
        gradientVector.y = (lossCostFunction(thetaPlus) - lossCostFunction(theta)) / step;

        // z
        thetaPlus = theta;
        thetaPlus.z = theta.z + step;
        gradientVector.z = (lossCostFunction(thetaPlus) - lossCostFunction(theta)) / step;

        // w
        thetaPlus = theta;
        thetaPlus.w = theta.w + step;
        gradientVector.w = (lossCostFunction(thetaPlus) - lossCostFunction(theta)) / step;


        gradientVector.Normalize();

        return gradientVector;
    }


}
