package com.defense.server.login;

import javax.mail.MessagingException;
import javax.mail.internet.MimeMessage;

import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.stereotype.Service;

@Service
public class LoginService {
    private JavaMailSender javaMailSender;

    public LoginService(JavaMailSender javaMailSender)
    {
		this.javaMailSender = javaMailSender;
    }

    public String sendOtpMail(String userEmail, int otpKey) {
        MimeMessage mimeMessage = javaMailSender.createMimeMessage();
        try {
            MimeMessageHelper mimeMessageHelper = new MimeMessageHelper(mimeMessage, true, "UTF-8");
            mimeMessageHelper.setTo(userEmail); // 메일 수신자
            mimeMessageHelper.setSubject("[System] Check second authentication"); // 메일 제목
            mimeMessageHelper.setText("OTP:"+otpKey, false); // 메일 본문 내용, HTML 여부
            mimeMessageHelper.setFrom("gesic84@gmail.com");
            javaMailSender.send(mimeMessage);
            return "Email Send Success!!";
        } catch (MessagingException e) {
            throw new RuntimeException(e);
        }
    }
}
