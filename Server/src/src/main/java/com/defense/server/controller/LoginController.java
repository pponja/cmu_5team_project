package com.defense.server.controller;

import java.time.LocalDateTime;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

import org.json.JSONObject;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import com.defense.server.entity.Users;
import com.defense.server.jwt.util.JwtUtil;
import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;
import com.defense.server.login.LoginConfig;
import com.defense.server.login.LoginService;
import com.defense.server.repository.UserRepository;

import lombok.Getter;
import lombok.RequiredArgsConstructor;
import lombok.Setter;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class LoginController {
	private final int PW_FAIL_COUNT = 5;
	private final long OTP_TIMEOUT_SEC = 60 * 2;
	
	private final UserRepository userRepository;
	private final JwtUtil jwtUtil;

    private final LoginService loginService;
    private final LoginConfig loginConfig;
    private final BCryptPasswordEncoder bcryptEncoder = new BCryptPasswordEncoder();
    
    @Getter
    @Setter
    private class OtpKey {
    	private final String KEY_OTP = "otp";
    	private final String KEY_EXPIRATION = "expiration";
    	
    	private int otp;
    	private LocalDateTime expiration;
    	
    	public OtpKey(int otp, LocalDateTime expiration) {
    		this.otp = otp;
    		this.expiration = expiration;
    	}
    	
    	public OtpKey(String jsonString) throws Exception {
    		JSONObject jsonObject = new JSONObject(jsonString);
    		otp = jsonObject.getInt(KEY_OTP);
    		expiration = LocalDateTime.parse(jsonObject.getString(KEY_EXPIRATION));
    	}
    	
    	@Override
    	public String toString() {
    		JSONObject jsonObject = new JSONObject();
    		jsonObject.put(KEY_OTP, otp);
    		jsonObject.put(KEY_EXPIRATION, expiration);
    		
    		return jsonObject.toString();
    	}
    }
    
    /*
     * !!!  C A U T I O N  !!!
     * This function is only added to help set the environment for Team 1.
     * In the real environment, this code does not exist,
     * and it is assumed that users are pre-registered in the DB.
     * Therefore, do not assume this code to be a vulnerability.
     * */
    
    @ResponseBody
    @PostMapping("/signup")
    public ResultJson signUp(@RequestBody Map<String, Object> recvInfo) {
    	ResultJson resultJson = new ResultJson();
    	
    	try {
    		if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
    		
    		Object userid = recvInfo.get("userid");
			Object password = recvInfo.get("password");
			Object email = recvInfo.get("email");
			if (userid == null || password == null || email == null) {
				throw new Exception("Invalid arguments");
			}
			
			Users user = userRepository.findUserByUserid(userid.toString());
			if (user != null) {
				throw new Exception("The user already exist");
			}
			
			user = new Users();
			user.setUserid(userid.toString());
			user.setPassword(bcryptEncoder.encode(password.toString()));
			user.setEmail(loginConfig.encrypt(email.toString()));
			userRepository.save(user);
			
			resultJson.setCode(ResultCode.SUCCESS.getCode());
			resultJson.setMsg(ResultCode.SUCCESS.getMsg());
			resultJson.setToken("");
    	} catch (Exception e) {
    		resultJson.setCode(ResultCode.SERVER_ERROR.getCode());
			resultJson.setMsg(ResultCode.SERVER_ERROR.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
    	}
    	
    	return resultJson;
    }
    
    
	@ResponseBody
	@PostMapping("/login")
	public ResultJson login(@RequestBody Map<String, Object> recvInfo) {
		ResultJson resultJson = new ResultJson();
		
		try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object userid = recvInfo.get("userid");
			Object password = recvInfo.get("password");
			if (userid == null || password == null) {
				throw new Exception("Invalid arguments");
			}
			
			Users user = userRepository.findUserByUserid(userid.toString());
			if (user == null) {
				throw new Exception("The user does not exist");
			}

			if (user.getFailcount() >= PW_FAIL_COUNT) {
				throw new Exception("Please contact administrator");
			}

			if (!bcryptEncoder.matches(password.toString(), user.getPassword())) {
				user.setFailcount(user.getFailcount() + 1);
				userRepository.save(user);
				throw new Exception("Invalid password, Fail Count:" + user.getFailcount());
			}
			
			user.setFailcount(0);
			sendMail(user);

			resultJson.setCode(ResultCode.SUCCESS.getCode());
			resultJson.setMsg(ResultCode.SUCCESS.getMsg());
			resultJson.setToken("");
		} catch (Exception e) {
			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
		}
		
		return resultJson;
	}
	
    private String sendMail(Users user) throws Exception {
    	if (user == null) {
    		throw new Exception("Invalid arguments");
    	}
    	
    	final String email = loginConfig.decrypt(user.getEmail());
    	final int otp = ThreadLocalRandom.current().nextInt(100000, 1000000);
    	
    	OtpKey otpKey = new OtpKey(otp, LocalDateTime.now().plusSeconds(OTP_TIMEOUT_SEC));
    	
    	final String encryptedOtpKey = loginConfig.encrypt(otpKey.toString());
    	user.setOtpKey(encryptedOtpKey);
    	userRepository.save(user);
    	
        return loginService.sendOtpMail(email, otp);
    }
    
    @ResponseBody
    @PostMapping("/otp-check")
    public ResultJson checkOtp(@RequestBody Map<String, Object> recvInfo) {
    	ResultJson resultJson = new ResultJson();
    	Users user;
    	try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object receivedUserid = recvInfo.get("userid");
			Object receivedOtp = recvInfo.get("otpKey");
			if (receivedUserid == null || receivedOtp == null) {
				throw new Exception("Invalid arguments");
			}
			
			user = userRepository.findUserByUserid(receivedUserid.toString());
			if (user == null) {
				throw new Exception("The user does not exist");
			}
			
			if (user.getOtpKey() == null) {
				throw new Exception("OTP session terminated");
			}
			
			String otpKeyString = loginConfig.decrypt(user.getOtpKey());
			OtpKey otpKey = new OtpKey(otpKeyString);
			if (otpKey.getExpiration().isBefore(LocalDateTime.now())) {
				user.setOtpKey(null);
				userRepository.save(user);
				throw new Exception("OTP session expired");
			}
			
			if (!receivedOtp.toString().equals(Integer.toString(otpKey.getOtp()))) {
				throw new Exception("Invalid OTP");
			}
			
			user.setOtpKey(null);
			userRepository.save(user);
	    	
    		String token = jwtUtil.generateToken(user);
    		
    		resultJson.setCode(ResultCode.SUCCESS.getCode());
    		resultJson.setMsg(ResultCode.SUCCESS.getMsg());
    		resultJson.setToken(token);
		} catch (Exception e) {
			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
		}

    	return resultJson;
   }
}