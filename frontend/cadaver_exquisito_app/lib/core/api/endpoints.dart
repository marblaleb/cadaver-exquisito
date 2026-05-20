class Endpoints {
  static const _base = 'http://10.0.2.2:5000/api'; // Android emulator → localhost

  static const register = '$_base/auth/register';
  static const updateFcmToken = '$_base/auth/fcm-token';
  static const availableCadavers = '$_base/cadavers/available';
  static const pendingCadavers = '$_base/cadavers/pending';
  static const completedCadavers = '$_base/cadavers/completed';
  static const cadavers = '$_base/cadavers';

  static String lastFragment(String id) => '$_base/cadavers/$id/last-fragment';
  static String addFragment(String id) => '$_base/cadavers/$id/fragments';
  static String fullCadaver(String id) => '$_base/cadavers/$id/full';
}
