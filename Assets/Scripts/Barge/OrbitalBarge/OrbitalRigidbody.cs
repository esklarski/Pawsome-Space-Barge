﻿using System;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    [SerializeField] private UpdateMethod method = UpdateMethod.FollowOrbit;
    [SerializeField] private float playerMultiplier = 2f;
    [SerializeField] private int maxContacts = 5;
    private ContactPoint2D[] contactArray;

    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private float CurrentTime => Time.fixedTime;

    private const float MaxAllowedDeltaV = 0.2f;

    public UpdateMethod Method => method;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();
        rb2d.isKinematic = method != UpdateMethod.Forces;
        if (rb2d.isKinematic) rb2d.useFullKinematicContacts = true;
        contactArray = new ContactPoint2D[maxContacts];
    }

    private void Start()
    {
        orbitalBody.Recalculate(CurrentTime);
        FollowOrbit();
    }

    private void FixedUpdate()
    {
        orbitalBody.Recalculate(CurrentTime);
        if (method == UpdateMethod.FollowOrbit)
        {
            FollowOrbit();
        }
        else
        {
            UseForces();
        }
    }

    private void UseForces()
    {
        orbitalBody.SetOrbit(CurrentTime, rb2d.position, rb2d.velocity);
        var gravity = orbitalBody.GravitationalForce * rb2d.mass;
        rb2d.AddForce(gravity, ForceMode2D.Force);
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private void FollowOrbit()
    {
        // check for bad values, hope skipping a frame doesn't make it worse :)
        if (
            float.IsNaN(orbitalBody.Velocity.x) || float.IsNaN(orbitalBody.Velocity.y)
            || float.IsNaN(orbitalBody.Position.x) || float.IsNaN(orbitalBody.Position.y)
        )
        { return; }

        rb2d.velocity = orbitalBody.Velocity;    // recieves { NaN, NaN }
        rb2d.MovePosition(orbitalBody.Position); // recieves { NaN, NaN }
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private Vector2 GetDeltaV(Collision2D col)
    {
        // Delta-V transferred from a collision = the impulse along the contact normal / the mass of this body
        float ourMass = rb2d.mass;

        var contactNum = col.GetContacts(contactArray);

        float impulse = 0;
        Vector2 normal = new Vector2(0,0);

        float multiplier = method == UpdateMethod.FollowOrbit ? playerMultiplier : playerMultiplier - 1;

        for (int i = 0; i < contactNum; i++)
        {
            var newImpulse = contactArray[i].rigidbody.CompareTag("Player")
                                ? contactArray[i].normalImpulse * multiplier
                                    : method == UpdateMethod.FollowOrbit
                                        ? contactArray[i].normalImpulse
                                            : 0;

            var newNormal = contactArray[i].rigidbody.CompareTag("Player")
                                ? contactArray[i].normal
                                    : method == UpdateMethod.FollowOrbit
                                        ? contactArray[i].normal
                                            : Vector2.zero;

            impulse += newImpulse;
            normal += newNormal;
        }

        return (impulse * normal) / (method == UpdateMethod.FollowOrbit ? ourMass : 1);
    }


    private void AddForce(Vector2 force)
    {
        if (method == UpdateMethod.Forces)
        {
            bool maxCheck = orbitalBody.CheckMaxRadius();
            bool minCheck = orbitalBody.CheckMinRadius();

            if (minCheck && maxCheck)
            {
                rb2d.AddForce(force, ForceMode2D.Impulse);
            }
            else
            {
                float forceDot = Vector2.Dot(force.normalized, rb2d.velocity.normalized);
                int forceModifier = 1;

                if (!maxCheck)
                {
                    if (forceDot > 0)
                    {
                        forceModifier = -1;
                    }
                }
                
                if (!minCheck)
                {
                    if (forceDot < 0)
                    {
                        forceModifier = -1;
                    }
                }

                rb2d.AddForce(force * forceModifier, ForceMode2D.Impulse);
            }
        }
        else
        {
            orbitalBody.AddDeltaV(CurrentTime, force);
        }
    }

    public void AddEnemyForce(float force)
    {
        AddForce(force * orbitalBody.Velocity.normalized);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Planet"))
        {
            Debug.Log("Barge hit planet.");
            GameManagement.Instance.MissionFailed();
        }

        AddForce(GetDeltaV(col));
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        AddForce(GetDeltaV(col));
    }
}

[Serializable]
public enum UpdateMethod
{
    Forces,
    FollowOrbit,
}
