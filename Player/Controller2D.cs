using UnityEngine;
using System.Collections;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class Controller2D : MonoBehaviour
    {
        // Basically Lague's Vanilla controller

        public LayerMask collisionMask;

        public float skinWidth = .05f;

        //public float horizRayLength = 1, vertRayLength = 1;
        const float dstBetweenRays = .25f;
        //[HideInInspector]
        public int horizontalRayCount = 10, verticalRayCount = 5;
        [HideInInspector]
        public float horizontalRaySpacing, verticalRaySpacing;
        [HideInInspector]
        public new BoxCollider2D collider;
        public RaycastOrigins raycastOrigins;

        [HideInInspector]
        public bool suspendGravity;

        // Controller2D
        public bool grounded;
        public bool climbing;
        public bool descending;
        public int facing;
        public Vector2 moveAmt;

        public float maxClimbAngle = 50;
        public float maxDescendAngle = 50;

        public CollisionInfo collisions;
        [HideInInspector]
        public Vector2 playerInput;

        Animator anim;
        [HideInInspector]
        public RaycastHit2D hit;

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        public struct CollisionInfo
        {
            public bool above, below;
            public bool left, right;
            public bool climbingSlope;
            public bool descendingSlope;
            public float slopeAngle, slopeAngleOld;
            public Vector2 moveAmountOld;
            public int faceDir;

            public void Reset()
            {
                above = below = false;
                left = right = false;
                climbingSlope = false;
                descendingSlope = false;

                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
            }
        }

        public void Awake()
        {
            //collider = GetComponent<CapsuleCollider2D>();
            collider = GetComponent<BoxCollider2D>();
        }

        public void Start()
        {
            CalculateRaySpacing();
            collisions.faceDir = 1;
        }

        private void Update()
        {
            descending = collisions.descendingSlope;
            climbing = collisions.climbingSlope;
            facing = collisions.faceDir;
            grounded = collisions.below;
        }


        public void UpdateRaycastOrigins()
        {
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            if (collider == null) return;
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);

            //float boundsWidth = bounds.size.x;
            //float boundsHeight = bounds.size.y;
            //horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
            //verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        public void Move(Vector2 moveAmount)
        {
            UpdateRaycastOrigins();

            collisions.Reset();
            collisions.moveAmountOld = moveAmount;

            moveAmt = moveAmount;

            if (moveAmount.x != 0)
            {
                collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
            }

            if (moveAmount.y < 0)
            {
                DescendSlope(ref moveAmount);
            }

            HorizontalCollisions(ref moveAmount);

            if (moveAmount.y != 0)
            {
                VerticalCollisions(ref moveAmount);
            }

            if (suspendGravity) moveAmount.y = 0;

            if (float.IsNaN(moveAmount.x) || float.IsNaN(moveAmount.y))
                return;
            else
                transform.Translate(moveAmount);
        }


        void HorizontalCollisions(ref Vector2 moveAmount)
        {
            float directionX = collisions.faceDir;
            float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            //float rayLength = horizRayLength;


            if (Mathf.Abs(moveAmount.x) < skinWidth)
            {
                rayLength = 2 * skinWidth;
            }

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                //Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);
                //Debug.DrawLine(rayOrigin, rayOrigin + (directionX * rayLength * Vector2.right), Color.red);

                Color gizmoColor = Color.red;

                if (hit)
                {
                    gizmoColor = Color.green;

                    if (hit.distance == 0) continue;
                    //if (PlayerManager.Instance.State == PlayerState.Boosting && BoostCanPassThrough(hit)) return;

                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= maxClimbAngle)
                    {
                        if (collisions.descendingSlope)
                        {
                            collisions.descendingSlope = false;
                            moveAmount = collisions.moveAmountOld;
                        }
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - skinWidth;
                            moveAmount.x -= distanceToSlopeStart * directionX;
                        }
                        ClimbSlope(ref moveAmount, slopeAngle);
                        moveAmount.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.climbingSlope)
                        {
                            moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                        }

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }

                Debug.DrawLine(rayOrigin, rayOrigin + (directionX * rayLength * Vector2.right), gizmoColor);
            }
        }

        void VerticalCollisions(ref Vector2 moveAmount)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;
            //float rayLength = vertRayLength;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                //Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);
                //Debug.DrawLine(rayOrigin, rayOrigin + (directionY * rayLength * Vector2.up), Color.red);

                Color gizmoColor = Color.red;

                // A vertical raycast hit a collider
                if (hit)
                {
                    gizmoColor = Color.green;

                    //PlayerManager.Instance.physics.vertRaycastHitRef = hit;

                    moveAmount.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                    }

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }

                Debug.DrawLine(rayOrigin, rayOrigin + (directionY * rayLength * Vector2.up), gizmoColor);
            }

            if (collisions.climbingSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }

        }

        void ClimbSlope(ref Vector2 moveAmount, float slopeAngle)
        {
            float moveDistance = Mathf.Abs(moveAmount.x);
            float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (moveAmount.y <= climbmoveAmountY)
            {
                moveAmount.y = climbmoveAmountY;
                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
            }
        }

        void DescendSlope(ref Vector2 moveAmount)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
            //Debug.DrawRay(rayOrigin, -Vector2.up, Color.yellow, 0.1f);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }
            }
        }

    }


}