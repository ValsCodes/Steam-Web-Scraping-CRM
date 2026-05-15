import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginComponent } from '../pages/login/login.component';
import { AuthService } from '../services';

describe('Login flow integration tests', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let authService: {
    login: jest.Mock;
    register: jest.Mock;
  };
  let router: {
    navigateByUrl: jest.Mock;
  };

  beforeEach(async () => {
    authService = {
      login: jest.fn(() => of({ token: 'token' })),
      register: jest.fn(() => of({ token: 'token' })),
    };
    router = {
      navigateByUrl: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('submits login credentials and navigates to the games page', () => {
    component.loginForm.patchValue({
      emailOrUserName: ' val@example.test ',
      password: 'Password1',
    });

    component.onSubmit();

    expect(authService.login).toHaveBeenCalledWith('val@example.test', 'Password1');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/games', { replaceUrl: true });
    expect(component.isSubmitting).toBe(false);
    expect(component.error).toBeNull();
  });

  it('shows validation errors before sending incomplete login forms', () => {
    component.loginForm.patchValue({
      emailOrUserName: '',
      password: '',
    });

    component.onSubmit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.error).toBe('Enter your username and password');
  });

  it('submits registration data only when password rules are satisfied', () => {
    component.setMode('register');
    component.loginForm.patchValue({
      email: 'admin@example.test',
      userName: 'admin',
      password: 'Password1',
      confirmPassword: 'Password1',
    });

    component.onSubmit();

    expect(authService.register).toHaveBeenCalledWith(
      'admin@example.test',
      'admin',
      'Password1',
      null,
      null,
      null,
    );
    expect(router.navigateByUrl).toHaveBeenCalledWith('/games', { replaceUrl: true });
  });

  it('keeps the user on the form and displays API validation messages', () => {
    authService.login.mockReturnValue(
      throwError(() =>
        new HttpErrorResponse({
          status: 400,
          error: { errors: { password: ['Password is invalid'] } },
        }),
      ),
    );
    component.loginForm.patchValue({
      emailOrUserName: 'val@example.test',
      password: 'wrong',
    });

    component.onSubmit();

    expect(router.navigateByUrl).not.toHaveBeenCalled();
    expect(component.error).toBe('Password is invalid');
  });
});
