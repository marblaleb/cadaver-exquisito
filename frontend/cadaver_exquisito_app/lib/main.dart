import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/providers/auth_provider.dart';
import 'features/auth/screens/login_screen.dart';
import 'features/cadavers/screens/main_scaffold.dart';

// In debug builds, Firebase is bypassed and the app goes straight to MainScaffold.
// Run `flutterfire configure` to generate firebase_options.dart for production.

void main() {
  runApp(const ProviderScope(child: CadaverExquisitoApp()));
}

class CadaverExquisitoApp extends ConsumerWidget {
  const CadaverExquisitoApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (kDebugMode) {
      return MaterialApp(
        title: 'Cadáver Exquisito',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.theme,
        home: const MainScaffold(),
      );
    }

    final authState = ref.watch(authProvider);
    return MaterialApp(
      title: 'Cadáver Exquisito',
      theme: AppTheme.theme,
      home: authState.when(
        data: (user) =>
            user != null ? const MainScaffold() : const LoginScreen(),
        loading: () => const Scaffold(
            body: Center(child: CircularProgressIndicator())),
        error: (_, __) => const LoginScreen(),
      ),
    );
  }
}
