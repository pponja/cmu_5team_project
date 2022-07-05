package com.defense.server.jwt.util;

import java.security.Key;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import com.defense.server.entity.Users;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.JwtException;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.io.Decoders;
import io.jsonwebtoken.io.Encoders;
import io.jsonwebtoken.security.Keys;

@Component
public class JwtUtil {
	@Value("${jwt.secret-key}")
	private String secretKey;
	
	private String createToken(Map<String, Object> claims) {
		String secretKeyEncodeBase64 = Encoders.BASE64.encode(secretKey.getBytes());
		byte[] keyBytes = Decoders.BASE64.decode(secretKeyEncodeBase64);
		Key key = Keys.hmacShaKeyFor(keyBytes);

		return Jwts.builder()
				.signWith(key)
				.setClaims(claims)
				.setIssuedAt(new Date(System.currentTimeMillis()))
				.compact();
	}

	private Claims extractAllClaims(String token) {
		if (token == null || token.isEmpty()) {
			return null;
		}
		
		String secretKeyEncodeBase64 = Encoders.BASE64.encode(secretKey.getBytes());
		Claims claims = null;
		
		try {
			claims = Jwts.parserBuilder()
					.setSigningKey(secretKeyEncodeBase64)
					.build()
					.parseClaimsJws(token)
					.getBody();
		} catch (JwtException e) {
			claims = null;
		}
		
		return claims;
	}

	public String extractUsername(String token) {
		final Claims claims = extractAllClaims(token);
		if (claims == null)
			return null;
		else
			return claims.get("userId", String.class);
	}

	public String generateToken(Users users) {
		Map<String, Object> claims = new HashMap<>();
		claims.put("userId", users.getUserid());
		return createToken(claims);
	}
}