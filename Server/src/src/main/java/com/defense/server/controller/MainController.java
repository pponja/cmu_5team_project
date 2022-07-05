package com.defense.server.controller;

import java.util.List;
import java.util.Map;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.ResponseBody;

import com.defense.server.Plate.PlateNumberService;
import com.defense.server.entity.Plateinfo;

import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
@Controller
@RequestMapping("/db")
public class MainController {

	private final PlateNumberService plateNumberService;
	 
	@RequestMapping(value = "/main", method = { RequestMethod.GET, RequestMethod.POST })
	public String home(Model model) {
		List<Plateinfo> plateNumberList = this.plateNumberService.getList();
		model.addAttribute("plateNumberList", plateNumberList);
		return "db";
	}

	@ResponseBody
	@RequestMapping(value = "/vehicle", method = RequestMethod.POST)
	public Plateinfo getResultJSON(@RequestBody Map<String, Object> recvInfo, Model model) {
		List<Plateinfo> result = this.plateNumberService.getQueryForPlateNumJSON(recvInfo.get("plateNum").toString());
		if (result == null || result.isEmpty())
		{
			return new Plateinfo();
		}
		return result.get(0);
	}
}
