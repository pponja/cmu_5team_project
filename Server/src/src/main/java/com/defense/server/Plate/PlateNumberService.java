package com.defense.server.Plate;

import java.util.Iterator;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.defense.server.entity.Plateinfo;
import com.defense.server.login.LoginConfig;
import com.defense.server.repository.PlateRepository;

import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
@Service
public class PlateNumberService {

	private final LoginConfig loginConfig;
	@Autowired
	private final PlateRepository plateRepository;

	public List<Plateinfo> getList() {
		List<Plateinfo> dbList = this.plateRepository.findAll();
		Iterator<Plateinfo> iterator = dbList.iterator();

		while(iterator.hasNext())
		{
			Plateinfo eachInfo = iterator.next();
			try {
				eachInfo.setLicensenumber(loginConfig.decrypt(eachInfo.getLicensenumber()));
				eachInfo.setLicensestatus(loginConfig.decrypt(eachInfo.getLicensestatus()));
				eachInfo.setLicenseexpdate(loginConfig.decrypt(eachInfo.getLicenseexpdate()));
				eachInfo.setOwnername(loginConfig.decrypt(eachInfo.getOwnername()));
				eachInfo.setOwnerbirthday(loginConfig.decrypt(eachInfo.getOwnerbirthday()));
				eachInfo.setOwneraddress(loginConfig.decrypt(eachInfo.getOwneraddress()));
				eachInfo.setOwnercity(loginConfig.decrypt(eachInfo.getOwnercity()));
				eachInfo.setVhemanufacture(loginConfig.decrypt(eachInfo.getVhemanufacture()));
				eachInfo.setVhemake(loginConfig.decrypt(eachInfo.getVhemake()));
				eachInfo.setVhemodel(loginConfig.decrypt(eachInfo.getVhemodel()));
				eachInfo.setVhecolor(loginConfig.decrypt(eachInfo.getVhecolor()));
			} catch (Exception e) {
				break;
			}
		}
		return dbList;
	}

	public List<Plateinfo> getQueryForPlateNumJSON(String platenum) {
		List<Plateinfo> searchresult = null;

		try {
			searchresult = this.plateRepository.findByLicensenumber(loginConfig.encrypt(platenum));

			if (searchresult == null)
			{
				throw new Exception("no plate");
			}
			searchresult.get(0).setLicensenumber(loginConfig.decrypt(searchresult.get(0).getLicensenumber()));
			searchresult.get(0).setLicensestatus(loginConfig.decrypt(searchresult.get(0).getLicensestatus()));
			searchresult.get(0).setLicenseexpdate(loginConfig.decrypt(searchresult.get(0).getLicenseexpdate()));
			searchresult.get(0).setOwnername(loginConfig.decrypt(searchresult.get(0).getOwnername()));
			searchresult.get(0).setOwnerbirthday(loginConfig.decrypt(searchresult.get(0).getOwnerbirthday()));
			searchresult.get(0).setOwneraddress(loginConfig.decrypt(searchresult.get(0).getOwneraddress()));
			searchresult.get(0).setOwnercity(loginConfig.decrypt(searchresult.get(0).getOwnercity()));
			searchresult.get(0).setVhemanufacture(loginConfig.decrypt(searchresult.get(0).getVhemanufacture()));
			searchresult.get(0).setVhemake(loginConfig.decrypt(searchresult.get(0).getVhemake()));
			searchresult.get(0).setVhemodel(loginConfig.decrypt(searchresult.get(0).getVhemodel()));
			searchresult.get(0).setVhecolor(loginConfig.decrypt(searchresult.get(0).getVhecolor()));
		} catch (Exception e) {
			Plateinfo nullPlateInfo = new Plateinfo();
			nullPlateInfo.setLicensenumber(platenum);
			searchresult.add(nullPlateInfo);
			return searchresult;
		}
		return searchresult;
	}
}