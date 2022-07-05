package com.defense.server.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.Table;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@Entity
@Table(name = "plateinfo")
public class Plateinfo {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;
    
	@Column(columnDefinition = "TEXT", unique = true)
    private String licensenumber;
	
	@Column(columnDefinition = "TEXT")
    private String licensestatus;
	
	@Column(columnDefinition = "TEXT")
	private String licenseexpdate;

	@Column(columnDefinition = "TEXT")
	private String ownername;

	@Column(columnDefinition = "TEXT")
	private String ownerbirthday;

	@Column(columnDefinition = "TEXT")
	private String owneraddress;

	@Column(columnDefinition = "TEXT")
	private String ownercity;

	@Column(columnDefinition = "TEXT")
	private String vhemanufacture;

	@Column(columnDefinition = "TEXT")
	private String vhemake;

	@Column(columnDefinition = "TEXT")
	private String vhemodel;

	@Column(columnDefinition = "TEXT")
	private String vhecolor;
	
}