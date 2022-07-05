package com.defense.server.repository;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;

import com.defense.server.entity.Plateinfo;

public interface PlateRepository extends JpaRepository<Plateinfo, Integer> {
	List<Plateinfo> findByLicensenumber(String licensenumber);
}