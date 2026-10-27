"""Customer identity from the platform JWT.

The assistant verifies the token itself (same issuer, audience and signing key
as the other services) and only ever looks up orders for the token's subject.
It never takes a user id from the request body or from the model, so one
customer cannot read another customer's orders by asking for them.
"""

from __future__ import annotations

from dataclasses import dataclass

import jwt

from .config import Settings


@dataclass(frozen=True)
class Customer:
    user_id: str
    email: str | None
    token: str


def customer_from_header(authorization: str | None, settings: Settings) -> Customer | None:
    if not authorization or not authorization.lower().startswith("bearer "):
        return None
    token = authorization[7:].strip()
    try:
        claims = jwt.decode(
            token,
            settings.jwt_signing_key,
            algorithms=["HS256"],
            audience=settings.jwt_audience,
            issuer=settings.jwt_issuer,
            options={"require": ["sub", "exp"]},
        )
    except jwt.PyJWTError:
        return None
    return Customer(user_id=str(claims["sub"]), email=claims.get("email"), token=token)
