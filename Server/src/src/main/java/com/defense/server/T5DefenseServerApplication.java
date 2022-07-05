package com.defense.server;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@EntityScan(basePackages = {"com.defense.server"})
@EnableJpaRepositories(basePackages = {"com.defense.server"})
@SpringBootApplication
public class T5DefenseServerApplication {

	public static void main(String[] args) {
		SpringApplication.run(T5DefenseServerApplication.class, args);
	}

}
