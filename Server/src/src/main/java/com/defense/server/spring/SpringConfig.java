package com.defense.server.spring;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.annotation.web.configuration.EnableWebSecurity;
import org.springframework.security.web.SecurityFilterChain;

@Configuration
@EnableWebSecurity
public class SpringConfig {

	@Bean
	protected SecurityFilterChain filterChain(HttpSecurity http) throws Exception
	{
		http.authorizeRequests().antMatchers("/h2-console/**").permitAll();
		http.csrf().ignoringAntMatchers("/h2-console/**").disable();
		http.headers().frameOptions().disable();

		return http.build();
	}
}