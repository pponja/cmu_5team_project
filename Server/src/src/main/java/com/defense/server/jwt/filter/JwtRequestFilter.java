package com.defense.server.jwt.filter;

import java.io.IOException;

import javax.servlet.FilterChain;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.springframework.security.core.userdetails.UsernameNotFoundException;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import com.defense.server.jwt.util.JwtUtil;
import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;
import com.defense.server.repository.UserRepository;
import com.fasterxml.jackson.databind.ObjectMapper;

import lombok.RequiredArgsConstructor;

@Component
@RequiredArgsConstructor
public class JwtRequestFilter extends OncePerRequestFilter {
    private final JwtUtil jwtUtil;
    private final UserRepository userRepository;

    @Override
    protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
            throws ServletException, IOException {

        String path = request.getRequestURI();

        if (path.startsWith("/auth") || path.startsWith("/h2-console")|| path.startsWith("/db/main")) {
            filterChain.doFilter(request, response);
            return;
        }

        final String authorizationHeader = request.getHeader("Authorization");
        String token = null;

        if (authorizationHeader != null && authorizationHeader.startsWith("Bearer ")) {
            token = authorizationHeader.substring(7);
        }
        
        String username = jwtUtil.extractUsername(token);
        if (username == null) {
            exceptionCall(response, "invalidToken");
            return;
        }
        
        try {
        	userRepository.findUserByUserid(username);
        } catch (UsernameNotFoundException e) {
        	exceptionCall(response, "invalidToken");
            return;
        }
        
        filterChain.doFilter(request, response);
    }

    private HttpServletResponse exceptionCall(HttpServletResponse response, String errorType) throws IOException {
        ResultJson resultJson = new ResultJson();
        if (errorType.equals("invalidToken")) {
            resultJson.setCode(ResultCode.INVALID_TOKEN.getCode());
            resultJson.setMsg(ResultCode.INVALID_TOKEN.getMsg());
        }

        ObjectMapper objectMapper = new ObjectMapper();
        response.getWriter().write(objectMapper.writeValueAsString(resultJson));
        response.setCharacterEncoding("utf-8");
        response.setContentType("application/json");
        return response;
    }
}