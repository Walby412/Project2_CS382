using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Slingshot : MonoBehaviour {
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    [SerializeField] private LineRenderer rubber;
    [SerializeField] private Transform firstPoint;
    [SerializeField] private Transform secondPoint;

    public AudioClip plopSound;
    private AudioSource audioSource;

    void Awake() {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseEnter() {
        // print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit() {
        // print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown() {
        aimingMode = true;
        projectile = Instantiate(projectilePrefab) as GameObject;
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        plopSound = Resources.Load<AudioClip>("plop_cut");
    }

    void Start() {
        rubber.SetPosition(0, firstPoint.position);
        rubber.SetPosition(2, secondPoint.position);
    }

    void Update() {
        if (!aimingMode) return;

        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - launchPos;
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude) {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // if (Input.GetMouseButton(0)){
        //     rubber.SetPosition(1, GetMousePositionInWorld());
        // }

        rubber.SetPosition(1, projPos);

        if (Input.GetMouseButtonUp(0)) {
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            MissionDemolition.SHOT_FIRED();

            audioSource.PlayOneShot(plopSound);

            StartCoroutine(SnapRubberBack());
        }
    }

    IEnumerator SnapRubberBack() {
        Vector3 middlePoint = (firstPoint.position + secondPoint.position) / 2;
        Vector3 currentPoint = rubber.GetPosition(1);

        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Vector3 newMiddlePos = Vector3.Lerp(currentPoint, middlePoint, t);
            rubber.SetPosition(1, newMiddlePos);

            yield return null;
        }

        rubber.SetPosition(1, middlePoint);
    }
}