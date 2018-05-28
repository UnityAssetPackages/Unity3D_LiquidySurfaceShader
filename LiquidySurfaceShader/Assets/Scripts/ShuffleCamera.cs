using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleCamera : MonoBehaviour {
    public bool isCamMoving;
    private Vector3 mCamLoc = new Vector3(0f, 0f, -1f);
    private Vector3 mCamLoc_target = new Vector3(0f, 0f, -1f);

    void Start () {
		
	}

    void Update() {
        if (Time.frameCount % 140 == (Mathf.Floor(Random.Range(0f, 1f) * 140.0f)))
        {
            ShuffleCam();
        }

        updateCamera();
    }

    private void ShuffleCam()
    {
        mCamLoc_target.x = (Random.Range(0f, 1f) * -0.5f - 0.5f);
        mCamLoc_target.y = (Random.Range(0f, 1f) * -0.5f - 0.5f);
        mCamLoc_target.z = (Random.Range(0f, 1f) * -0.5f - 0.5f);

        mCamLoc_target.Normalize();
        mCamLoc_target *= (Random.Range(0f, 1f) * 0.2f + 0.9f);
    }

    private void updateCamera()
    {
        Vector3 dir = mCamLoc_target - mCamLoc;
        float dist = dir.magnitude;
        dir.Normalize();

        if (dist < 0.1f)
        {
            mCamLoc = mCamLoc_target;

            ShuffleCam();
        }
        else
        {
            mCamLoc += dir * dist * .02f;
        }

        transform.position = mCamLoc;
        transform.LookAt(Vector3.zero, Vector3.up);
    }
}
