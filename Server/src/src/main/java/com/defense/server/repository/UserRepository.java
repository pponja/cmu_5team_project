package com.defense.server.repository;

import org.springframework.data.jpa.repository.JpaRepository;

import com.defense.server.entity.Users;

public interface UserRepository extends JpaRepository<Users, Long> {
	Users findUserByUserid(String userid);
}