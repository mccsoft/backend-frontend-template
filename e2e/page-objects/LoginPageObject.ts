import { Page } from '@playwright/test';
import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';

export class LoginPageObject extends PageObjectBase {
  constructor(page: Page) {
    super(page.getByTestId('login-container'));
  }

  loginField = () => this.findField('Login');
  passwordField = () => this.findField('Password');
  loginButton = () =>
    this.region.getByRole('button').filter({ hasText: 'Login' });

  async fill(data: { login: string; password: string }) {
    await this.loginField().fillTextField(data.login);
    await this.passwordField().fillTextField(data.password);
  }

  async logIn(login: string, password: string) {
    await this.fill({ login, password });
    await this.loginButton().click();
    await this.waitForLoadings();
  }
}
